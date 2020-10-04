using Love;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using File = System.IO.File;

namespace CodeEditor.UI
{
    class TextEditor : Window
    {
        public Point Cursor = new Point();

        public int CounterWidth = 50;
        public int CounterBorderSpace = 10;

        public HighlighterTheme HighlighterTheme;
        public HighlighterParser HighlighterParser;

        public List<string> Lines = new List<string>();
        public string Text {
            get {
                return string.Join( "\r\n", Lines );
            }
            set {
                Lines.Clear();

                int max_width = 0;
                foreach ( string line in value.Split( "\r\n" ) )
                {
                    max_width = Math.Max( max_width, TextFont.GetWidth( line ) );
                    Lines.Add( line );
                }
                TextWidth = max_width;
            }
        }
        public int TextWidth;

        public string FilePath = "";

        public TextEditor() {
            Bounds = new Rectangle( 0, 0, 450, 300 );

            SetRightTitle( () =>
            {
                return string.Format( "L={0} C={1}", Cursor.Y + 1, Cursor.X + 1 );
            } );

            ComputeFontHeight();
        }

        public void SetFile( string path )
        {
            Text = File.ReadAllText( path );
            Title = Path.GetFileName( path );
            FilePath = path;
            Main.Log( string.Format( "Load '{0}'", path ) );

            //  > Reset Cursor
            Cursor.X = 0;
            Cursor.Y = 0;

            //  > Reset Camera
            Camera.X = 0;
            Camera.Y = 0;

            //  > Get Highlighter
            var highlighter = HighlighterParser.Get( Path.GetExtension( path ).Replace( ".", "" ) );
            if ( !( highlighter == null ) )
                HighlighterParser = highlighter;
        }

        public void Save()
        {
            File.WriteAllLines( FilePath, Lines );
            Main.Log( string.Format( "Saved '{0}'", FilePath ) );
        }

        public int GetClampedCursorX( int x ) => Math.Clamp( x, 0, Math.Max( Lines[Cursor.Y].Length, 0 ) );
        public int GetClampedCursorY( int y ) => Math.Clamp( y, 0, Lines.Count - 1 );

        public void SetCursorPos( int x, int y )
        {
            Cursor.Y = GetClampedCursorY( y );
            Cursor.X = GetClampedCursorX( x );
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
                if ( current_color == HighlighterTheme.Comment )
                    current_color = TextColor;

            if ( word.Length == 1 && HighlighterParser.String.Contains( word.ToString() ) )
                if ( current_color == HighlighterTheme.String )
                {
                    current_color = TextColor;
                    return HighlighterTheme.String;
                }
                else
                    current_color = HighlighterTheme.String;
            else if ( HighlighterParser.Comment.Contains( word ) )
                current_color = HighlighterTheme.Comment;

            if ( !( current_color == TextColor ) )
                return current_color;

            if ( HighlighterParser.Syntax.Contains( word ) )
                return HighlighterTheme.Syntax;
            //if ( /*next_word == "(" &&*/ HighlighterParser.Function.Contains( word ) )
            //    return HighlighterTheme.Function;
            if ( int.TryParse( word, out _ ) || HighlighterParser.Bool.Contains( word ) )
                return HighlighterTheme.Number;

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

        public override void WheelMoved( int x, int y )
        {
            var speed = -y * 50;

            //  > Scroll X
            if ( Keyboard.IsDown( KeyConstant.LShift ) )
                Camera.X = Math.Clamp( Camera.X + speed, 0, TextWidth );
            //  > Scroll Y
            else
                Camera.Y = Math.Clamp( Camera.Y + speed, 0, Lines.Count * LineHeight - TitleHeight );
        }

        public override void KeyPressed( KeyConstant key, Scancode scancode, bool is_repeat )
        {
            //Console.WriteLine( key );

            //  > Save
            if ( Keyboard.IsDown( KeyConstant.LCtrl ) && Keyboard.IsDown( KeyConstant.S ) )
            {
                Save();
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
                }
        }

        public override void TextInput( string text )
        {
            Lines[Cursor.Y] = Lines[Cursor.Y].Insert( Cursor.X, text );
            Cursor.X++;
            //Console.WriteLine( "insert '{0}'", text );
        }

        public override void InnerRender()
        {
            //  > Counter Border
            Graphics.SetColor( DiscretColor );
            Graphics.Line( CounterWidth - Camera.X, Padding.Y - Camera.Y, CounterWidth - Camera.X, Padding.Y + LineHeight * Lines.Count - Padding.Z - Camera.Y );

            //  > Text
            Graphics.Translate( 0, TitleHeight / 4 );
            Graphics.SetFont( TextFont );
            for ( int i = 0; i < Lines.Count; i++ )
            {
                //  > Counter
                Graphics.SetColor( DiscretColor );
                Graphics.Printf( ( i + 1 ).ToString(), -Camera.X, LineHeight * i - Camera.Y, CounterWidth, AlignMode.Center );

                //  > Line
                int off_x = 0;
                MatchCollection matches = Regex.Matches( Lines[i], @"\w+|--|\W" );
                for ( int u = 0; u < matches.Count; u++ )
                {
                    Match match = matches[u];
                    string word = match.Value;

                    Graphics.SetColor( GetWordColor( word, u == 0/*, u + 1 < matches.Count ? matches[u + 1].Value : ""*/ ) );
                    Graphics.Print( word, CounterWidth + CounterBorderSpace - Camera.X + off_x, LineHeight * i - Camera.Y );
                    off_x += TextFont.GetWidth( word );
                }
            }

            //  > Cursor
            if ( !IsFocus ) return;
            Graphics.SetColor( TextColor.r, TextColor.g, TextColor.b, .5f );
            Graphics.Rectangle( Timer.GetTime() % 1 <= .5 ? DrawMode.Fill : DrawMode.Line, CounterWidth + CounterBorderSpace - Camera.X + GetCursorX(), -Camera.Y + Cursor.Y * LineHeight, GetCursorCharWide(), LineHeight );
        }
    }
}
