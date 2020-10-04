﻿using Love;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using File = System.IO.File;
using System.Windows;
using Point = Love.Point;
using Dapplo.Log;
using System.Diagnostics;

namespace CodeEditor.UI
{
    class TextEditor : Window
    {
        public Point Cursor = new Point();

        public int CounterWidth = 50;
        public int CounterBorderSpace = 10;

        public HighlighterParser HighlighterParser;

        public List<string> Lines = new List<string>() { "" };
        public string Text {
            get {
                return string.Join( "\r\n", Lines );
            }
            set {
                Lines.Clear();

                foreach ( string line in value.Split( "\r\n" ) )
                    Lines.Add( line );
            }
        }

        public string FilePath = "";

        public TextEditor() {
            Bounds = new Rectangle( 0, 0, 450, 300 );

            SetRightTitle( () =>
            {
                return string.Format( "L={0} C={1}", Cursor.Y + 1, Cursor.X + 1 );
            } );

            ///  > Adding Buttons
            //  > Save
            Children.Add( new WindowButton( 1, this, ( WindowButton self ) => ( (TextEditor) self.Parent ).Save() ) );
            //  > Load
            Children.Add( new WindowButton( 2, this, ( WindowButton self ) => ( (TextEditor) self.Parent ).Load() ) );
        }

        public void SetFile( string path )
        {
            try
            {
                Text = File.ReadAllText( path );
                Title = Path.GetFileName( path );
                FilePath = path;
                Main.Log( string.Format( "File: load '{0}'", path ) );
            }
            catch ( IOException )
            {
                Main.Log( string.Format( "ERROR: failed loading '{0}'", path ) );
            }

            //  > Reset Cursor
            Cursor.X = 0;
            Cursor.Y = 0;

            //  > Reset Camera
            Camera.X = 0;
            Camera.Y = 0;

            //  > Get Highlighter
            var extension = Path.GetExtension( FilePath ).Replace( ".", "" );
            var highlighter = HighlighterParser.Get( extension );
            if ( !( highlighter == null ) )
            {
                HighlighterParser = highlighter;
                if ( extension == "py" )
                {
                    Children.Add( new WindowButton( 4, this, ( WindowButton self ) => Run() ) );
                    ComputeLayout();
                }
            }
        }

        public void Save()
        {
            File.WriteAllLines( FilePath, Lines );
            Main.Log( string.Format( "Saved '{0}'", FilePath ) );
        }

        public void Load()
        {
            Main.Log( "ERROR: not implemented" );   
        }

        public void Run()
        {
            //Main.Log( string.Format( "Run '{0}'", FilePath ) );
            if ( !FilePath.EndsWith( "py" ) ) return;
            Main.Log( string.Format( "> '{0}' '{1}'", Program.Preferences.Interpreters.Python, FilePath ) );

            var process = new Process()
            {
                StartInfo =
                {
                    FileName = Program.Preferences.Interpreters.Python,
                    Arguments = FilePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };

            process.OutputDataReceived += Main.DevConsole.OutputHandler;
            process.ErrorDataReceived += Main.DevConsole.OutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        public int GetClampedCursorX( int x ) => Math.Clamp( x, 0, Math.Max( Lines[Cursor.Y].Length, 0 ) );
        public int GetClampedCursorY( int y ) => Math.Clamp( y, 0, Lines.Count - 1 );

        public void SetCursorPos( int x, int y )
        {
            Cursor.Y = GetClampedCursorY( y );
            Cursor.X = GetClampedCursorX( x );

            var cursor_y = (int) ( Cursor.Y * LineHeight - Camera.Y ) + TitleHeight * 2;
            if ( !Intersect( (int) ( Cursor.X - Camera.X ), cursor_y ) )
            {
                Console.WriteLine( "hey" );
                SetCameraY( Camera.Y + ( cursor_y > Bounds.Y + Bounds.Height ? LineHeight : -LineHeight ) );
            }
        }

        public void MoveCursorTowards( int x, int y )
        {
            //  > Reset X
            if ( y < 0 )
                if ( Cursor.Y == 0 )
                    Cursor.X = 0;
            //  > Move Y
            if ( !( y == 0 ) )
                SetCursorPos( Cursor.X, Cursor.Y + y );
            
            //  > Move X
            if ( !( x == 0 ) )
            {
                var new_x = GetClampedCursorX( Cursor.X + x );

                //  > Go to next line
                if ( Cursor.X == new_x )
                {
                    //  > Get new X
                    if ( x < 0 && Cursor.Y + x >= 0 )
                        new_x = Lines[Cursor.Y + x].Length;
                    else
                        new_x = 0;

                    SetCursorPos( new_x, Cursor.Y + x );
                }
                //  > Go to next char
                else 
                    Cursor.X = new_x;
            }
        }

        Color current_color;
        public Color GetWordColor( string word, bool new_line = false/*, string next_word = ""*/ )
        {
            if ( HighlighterParser == null )
                return TextColor;

            word = word.Trim();

            if ( new_line )
                if ( current_color == Main.CurrentTheme.Highlighter.Comment )
                    current_color = TextColor;

            if ( word.Length == 1 && HighlighterParser.String.Contains( word.ToString() ) )
                if ( current_color == Main.CurrentTheme.Highlighter.String )
                {
                    current_color = TextColor;
                    return Main.CurrentTheme.Highlighter.String;
                }
                else
                    current_color = Main.CurrentTheme.Highlighter.String;
            else if ( HighlighterParser.Comment.Contains( word ) )
                current_color = Main.CurrentTheme.Highlighter.Comment;

            if ( !( current_color == TextColor ) )
                return current_color;

            if ( HighlighterParser.Syntax.Contains( word ) )
                return Main.CurrentTheme.Highlighter.Syntax;
            //if ( /*next_word == "(" &&*/ HighlighterParser.Function.Contains( word ) )
            //    return Main.CurrentTheme.Highlighter.Function;
            if ( int.TryParse( word, out _ ) || HighlighterParser.Bool.Contains( word ) )
                return Main.CurrentTheme.Highlighter.Number;

            return TextColor;
        }

        public string GetSafeLine( int y )
        {
            if ( y >= Lines.Count - 1 )
                return " ";

            return Regex.Replace( Lines[y], @"\p{C}+", " " );
        }

        public int GetCursorX() =>  TextFont.GetWidth( Lines[Cursor.Y].Substring( 0, Cursor.X ) );
        public int GetCursorCharWide()
        {
            string line = GetSafeLine( Cursor.Y );
            if ( Cursor.X >= line.Length )
                return TextFont.GetWidth( " " );

            string cursor_char = line[Cursor.X].ToString();
            return TextFont.GetWidth( cursor_char );
        }

        public void Append( string text )
        {
            if ( text.Contains( "\n" ) )
            {
                string start_line_text = Lines[Cursor.Y].Remove( 0, Cursor.X );
                Lines[Cursor.Y] = Lines[Cursor.Y].Substring( 0, Cursor.X );

                foreach ( string line in text.Split( "\n" ) )
                {
                    Append( line );
                    Lines.Insert( Cursor.Y + 1, "" );
                    SetCursorPos( 0, Cursor.Y + 1 );
                }

                Append( start_line_text );
            }
            else
                Lines[Cursor.Y] = Lines[Cursor.Y].Insert( Cursor.X, text );
        }

        ///  > Camera
        public void SetCameraX( float cam_x )
        {
            Camera.X = Math.Clamp( cam_x, 0, Lines.Aggregate( 0, ( acc, x ) => Math.Max( TextFont.GetWidth( x ), acc ) ) * .75f - CounterWidth );
        }
        public void SetCameraY( float cam_y )
        {
            Camera.Y = Math.Clamp( cam_y, 0, LineHeight * Lines.Count * .95f );
        }

        public override void WheelMoved( int x, int y )
        {
            var speed = -y * 50;

            //  > Scroll X
            if ( Keyboard.IsDown( KeyConstant.LShift ) )
                SetCameraX( Camera.X + speed );
            //Camera.X = Math.Clamp( Camera.X + speed, 0, Lines.Aggregate( 0, ( acc, x ) => Math.Max( TextFont.GetWidth( x ), acc ) ) * .75f - CounterWidth );
            //  > Scroll Y
            else
                Camera.Y = Math.Clamp( Camera.Y + speed, 0, LineHeight * Lines.Count * .95f );
        }

        public override void KeyPressed( KeyConstant key, Scancode scancode, bool is_repeat )
        {
            //Console.WriteLine( key );

            //  > Control keys
            if ( Keyboard.IsDown( KeyConstant.LCtrl ) )
            {
                //  > Save
                if ( Keyboard.IsDown( KeyConstant.S ) )
                    Save();
                else if ( Keyboard.IsDown( KeyConstant.V ) )
                {
                    string text = Clipboard.GetText();
                    Append( text );

                    if ( !text.Contains( "\n" ) )
                        MoveCursorTowards( text.Length, 0 );
                }
            }
            //  > Single keys
            else
                switch ( key )
                {
                    //  > Cursor Movements
                    case KeyConstant.Up:
                        MoveCursorTowards( 0, -1 );
                        break;
                    case KeyConstant.Down:
                        MoveCursorTowards( 0, 1 );
                        break;
                    case KeyConstant.Left:
                        MoveCursorTowards( -1, 0 );
                        break;
                    case KeyConstant.Right:
                        MoveCursorTowards( 1, 0 );
                        break;
                    //  > Editing
                    case KeyConstant.Enter:
                        //  > Create new line with text
                        Lines.Insert( Cursor.Y + 1, Lines[Cursor.Y].Substring( Cursor.X ) );

                        //  > Remove old text
                        Lines[Cursor.Y] = Lines[Cursor.Y].Substring( 0, Cursor.X );

                        //  > Set cursor pos to new line
                        SetCursorPos( 0, Cursor.Y + 1 );
                        break;
                    case KeyConstant.KeypadEnter:
                        KeyPressed( KeyConstant.Enter, scancode, is_repeat );
                        break;
                    case KeyConstant.Tab:
                        var n = 4;
                        Lines[Cursor.Y] = Lines[Cursor.Y].Insert( Cursor.X, new String( ' ', n ) );
                        MoveCursorTowards( n, 0 );
                        break;
                    case KeyConstant.Backspace:
                        //  > Remove line
                        if ( Cursor.X == 0 )
                        {
                            if ( Cursor.Y > 0 )
                            {
                                int new_x = Lines[Cursor.Y - 1].Length;
                                Lines[Cursor.Y - 1] = Lines[Cursor.Y - 1].Insert( Lines[Cursor.Y - 1].Length, Lines[Cursor.Y] );
                                Lines.RemoveAt( Cursor.Y );
                                SetCursorPos( new_x, Cursor.Y - 1 );
                            }
                        }
                        //  > Remove chars
                        else
                        {
                            Lines[Cursor.Y] = Lines[Cursor.Y].Remove( Cursor.X - 1, 1 );
                            MoveCursorTowards( -1, 0 );
                        }

                        break;
                    case KeyConstant.Delete:
                        //  > Remove line
                        if ( Cursor.X == Lines[Cursor.Y].Length )
                        {
                            if ( Cursor.Y < Lines.Count - 1 )
                            {
                                Lines[Cursor.Y] = Lines[Cursor.Y].Insert( Lines[Cursor.Y].Length, Lines[Cursor.Y + 1] );
                                Lines.RemoveAt( Cursor.Y + 1 );
                            }
                        }
                        //  > Remove chars
                        else
                        {
                            Lines[Cursor.Y] = Lines[Cursor.Y].Remove( Cursor.X, 1 );
                        }

                        break;
                    case KeyConstant.F5:
                        Save();
                        Run();
                        break;
                }
        }

        public override void TextInput( string text )
        {
            Append( text );
            Cursor.X++;
            //Console.WriteLine( "insert '{0}'", text );
        }

        public override void InnerRender()
        {
            //  > Text
            Graphics.Translate( 0, TitleHeight / 4 );
            Graphics.SetFont( TextFont );
            for ( int i = 0; i < Lines.Count; i++ )
            {
                //  > Line
                int off_x = 0;
                MatchCollection matches = Regex.Matches( Lines[i], @"\w+|--|\W" );
                for ( int u = 0; u < matches.Count; u++ )
                {
                    Match match = matches[u];
                    string word = match.Value;

                    Graphics.SetColor( GetWordColor( word, u == 0/*, u + 1 < matches.Count ? matches[u + 1].Value : ""*/ ) );
                    Graphics.Print( word, (int) ( CounterWidth + CounterBorderSpace - Camera.X + off_x ), (int) ( LineHeight * i - Camera.Y ) );
                    off_x += TextFont.GetWidth( word );
                }
            }

            ///  > Cursor
            if ( IsFocus )
            {
                Graphics.SetColor( TextColor.r, TextColor.g, TextColor.b, .5f );
                Graphics.Rectangle( Timer.GetTime() % 1 <= .5 ? DrawMode.Fill : DrawMode.Line, CounterWidth + CounterBorderSpace - Camera.X + GetCursorX(), -Camera.Y + Cursor.Y * LineHeight, GetCursorCharWide(), LineHeight );
            }

            ///  > Counter
            //  > Background
            Graphics.SetColor( BackgroundColor );
            Graphics.Rectangle( DrawMode.Fill, 0, -TitleHeight, CounterWidth, Bounds.Height );

            //  > Border
            Graphics.SetColor( DiscretColor );
            Graphics.Line( CounterWidth, -Camera.Y, CounterWidth, LineHeight * Lines.Count - Padding.Z - Camera.Y );

            //  > Lines
            for ( int i = 0; i < Lines.Count; i++ )
            {
                //  > Counter
                Graphics.SetColor( DiscretColor );
                Graphics.Printf( ( i + 1 ).ToString(), 0, (int) ( LineHeight * i - Camera.Y ), CounterWidth, AlignMode.Center );
            }
        }
    }
}
