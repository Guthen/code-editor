using Love;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using File = System.IO.File;
using System.Windows;
using Point = Love.Point;
using System.Diagnostics;

namespace CodeEditor.UI
{
    class TextEditor : Window
    {
        public Point Cursor = new Point();

        public WindowButton RunButton;

        public int CounterWidth = 50;
        public int CounterBorderSpace = 10;

        public Language Language;

        public List<string> Lines = new List<string>() { "" };
        public string Text {
            get {
                return string.Join( "\r\n", Lines );
            }
            set {
                Lines.Clear();

                var lines = value.Split( "\r\n" );
                for ( int i = 0; i < lines.Length; i++ )
                    if ( lines[i].Length > 0 || i < lines.Length - 1 )
                        Lines.Add( lines[i] );

                if ( Lines.Count == 0 ) Lines.Add( "" );
            }
        }

        public string FilePath = "";
        public string RunKey;

        public TextEditor() {
            Bounds = new Rectangle( 0, 0, 450, 300 );

            SetRightTitle( () =>
            {
                return string.Format( "L={0} C={1} IDE={2}", Cursor.Y + 1, Cursor.X + 1, RunKey.Length <= 0 ? "NONE" : RunKey );
            } );

            ///  > Adding Buttons
            //  > Save
            Children.Add( new WindowButton( 1, this, ( WindowButton self ) => Save() ) );
            //  > Load
            Children.Add( new WindowButton( 2, this, ( WindowButton self ) => Load() ) );
            //  > Run
            RunButton = new WindowButton( 4, this, ( WindowButton self ) => Run() );
            RunButton.Visible = false;
        }

        public void SetFile( string path )
        {
            try
            {
                Text = File.ReadAllText( path );
                Title = Path.GetFileName( path );
                FilePath = path;
                Boot.Log( string.Format( "File: load '{0}'", path ) );
            }
            catch ( IOException )
            {
                Boot.Log( string.Format( "ERROR: failed loading '{0}'", path ) );
            }

            //  > Reset Cursor
            Cursor.X = 0;
            Cursor.Y = 0;

            //  > Reset Camera
            Camera.X = 0;
            Camera.Y = 0;

            //  > Get Highlighter
            var extension = Path.GetExtension( FilePath ).Replace( ".", "" );
            var language = Language.Get( extension );
            if ( !( language == null ) )
            {
                Language = language;
                RunKey = Language.RunCommands.First().Key;

                //  > Add/Remove Run Button
                if ( Program.Preferences.Interpreters.ContainsKey( extension ) )
                {
                    if ( !Children.Contains( RunButton ) )
                    {
                        Children.Add( RunButton );
                        RunButton.Visible = true;
                        ComputeLayout();
                    }
                }
                else
                {
                    if ( Children.Contains( RunButton ) )
                    {
                        Children.Remove( RunButton );
                        RunButton.Visible = false;
                        ComputeLayout();
                    }
                }
            }
            else
                Language = new Language()
                {
                    Highlighter = new HighlighterParser(),
                };
        }

        public void Save()
        {
            if ( FilePath.Length <= 0 ) return;
            File.WriteAllLines( FilePath, Lines );
            Boot.Log( string.Format( "Saved '{0}'", FilePath ) );
        }

        public void Load()
        {
            Boot.Log( "ERROR: not implemented" );   
        }

        public void Run()
        {
            //  > Get run command
            var cmd = Language.RunCommands.ContainsKey( RunKey ) ? Language.RunCommands[RunKey] : null;
            if ( cmd == null ) return;

            //  > Check Interpreter Preference 
            var extension = Path.GetExtension( FilePath ).Replace( ".", "" );
            if ( !Program.Preferences.Interpreters.ContainsKey( extension ) || !Program.Preferences.Interpreters[extension].ContainsKey( RunKey ) ) {
                Boot.Log( string.Format( "ERROR: not configured for '.{0}' ({1})", extension, RunKey ) );
                return;
            };

            //  > Replace Variables
            var run_cmd = cmd.Replace( "%FilePath%", FilePath )
                             .Replace( "%FolderPath%", Path.GetFullPath( "../", FilePath ) );

            //  > Get Interpreter
            var interpreter = Program.Preferences.Interpreters[extension][RunKey];

            //  > Start Process
            Boot.Log( string.Format( "> '{0}' '{1}'", interpreter, run_cmd ) );
            var process = new Process()
            {
                StartInfo =
                {
                    FileName = interpreter,
                    Arguments = run_cmd,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };

            process.OutputDataReceived += Boot.DevConsole.OutputHandler;
            process.ErrorDataReceived += Boot.DevConsole.OutputHandler;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch ( Exception e )
            {
                Boot.Log( "ERROR: " + e.Message );
            }
        }

        public int GetClampedCursorX( int x ) => Math.Clamp( x, 0, Math.Max( Lines[Cursor.Y].Length, 0 ) );
        public int GetClampedCursorY( int y ) => Math.Clamp( y, 0, Lines.Count - 1 );

        public void SetCursorPos( int x, int y )
        {
            Cursor.Y = GetClampedCursorY( y );
            Cursor.X = GetClampedCursorX( x );

            //  > Cursor Move on Y-Axis
            var cursor_y = Cursor.Y * LineHeight;
            var view_height = Bounds.Height - TitleHeight * 2 - ( Padding.Y + Padding.Z );
            if ( cursor_y < Camera.Y )
                SetCameraY( cursor_y );
            else if ( cursor_y > Camera.Y + view_height )
                SetCameraY( cursor_y - view_height );

            //  > Cursor Move on X-Axis
            var cursor_x = GetCursorX();
            var view_width = Bounds.Width - ( CounterWidth + CounterBorderSpace ) - ( Padding.X + Padding.W ) - GetCursorCharWide() * 2;
            if ( cursor_x < Camera.X )
                SetCameraX( cursor_x );
            else if ( cursor_x > Camera.X + view_width )
                SetCameraX( cursor_x - view_width );
        }

        public void MoveCursorTowards( int x, int y )
        {
            //  > Reset X at top
            if ( y < 0 )
            {
                if ( Cursor.Y == 0 )
                    Cursor.X = 0;
            }
            //  > Max X at bottom
            else if ( y > 0 )
                if ( Cursor.Y == Lines.Count - 1 )
                    Cursor.X = Lines[Cursor.Y].Length;

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
                    SetCursorPos( new_x, Cursor.Y );
            }
        }

        bool is_multiline_comment;
        Color current_color;
        public Color GetWordColor( string word, bool new_line = false/*, string next_word = ""*/ )
        {
            if ( Language == null || !Language.Success )
                return TextColor;

            word = word.Trim();

            //  > End of Multi-Line Comment
            if ( Language.IsEndMultilineComment( word ) && is_multiline_comment )
            {
                is_multiline_comment = false;
                current_color = TextColor;
                return Boot.CurrentTheme.Highlighter.Comment;
            }
            //  > End of Inline-Comment Color
            if ( new_line )
            {
                if ( current_color == Boot.CurrentTheme.Highlighter.Comment && !is_multiline_comment )
                    current_color = TextColor;
            }
            //  > Get Comment Color
            else if ( current_color == Boot.CurrentTheme.Highlighter.Comment )
                return current_color;

            //  > String
            if ( word.Length == 1 && Language.IsString( word ) )
                if ( current_color == Boot.CurrentTheme.Highlighter.String )
                {
                    current_color = TextColor;
                    return Boot.CurrentTheme.Highlighter.String;
                }
                else
                    current_color = Boot.CurrentTheme.Highlighter.String;
            //  > Comment
            else if ( Language.IsInlineComment( word ) )
                current_color = Boot.CurrentTheme.Highlighter.Comment;
            //  > Multi-line Comment
            else if ( Language.IsStartMultilineComment( word ) )
            {
                current_color = Boot.CurrentTheme.Highlighter.Comment;
                is_multiline_comment = true;
            }

            if ( current_color == Boot.CurrentTheme.Highlighter.Comment || current_color == Boot.CurrentTheme.Highlighter.String )
                return current_color;

            //  > Syntax
            if ( Language.IsSyntax( word ) )
                return Boot.CurrentTheme.Highlighter.Syntax;
            //if ( /*next_word == "(" &&*/ HighlighterParser.Function.Contains( word ) )
            //    return Main.CurrentTheme.Highlighter.Function;
            //  > Number and Booleans
            if ( int.TryParse( word, out _ ) || Language.IsBool( word ) )
                return Boot.CurrentTheme.Highlighter.Number;

            return TextColor;
        }

        public string GetSafeLine( int y )
        {
            if ( y >= Lines.Count - 1 )
                return " ";

            return Regex.Replace( Lines[y], @"\p{C}+", " " );
        }

        public int GetCursorX() => TextFont.LFont.GetWidth( Lines[Cursor.Y].Substring( 0, Cursor.X ) );
        public int GetCursorCharWide()
        {
            string line = GetSafeLine( Cursor.Y );
            if ( Cursor.X >= line.Length )
                return TextFont.LFont.GetWidth( " " );

            string cursor_char = line[Cursor.X].ToString();
            return TextFont.LFont.GetWidth( cursor_char );
        }

        public void SwapLine( int y )
        {
            if ( Cursor.Y + y < 0 || Cursor.Y + y > Lines.Count - 1 ) return;

            string text = Lines[Cursor.Y];
            Lines.RemoveAt( Cursor.Y );
            Lines.Insert( Cursor.Y + y, text );
            SetCursorPos( Cursor.X, Cursor.Y + y );
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
            Camera.X = Math.Clamp( cam_x, 0, Lines.Aggregate( 0, ( acc, x ) => Math.Max( TextFont.LFont.GetWidth( x ), acc ) ) * .75f - CounterWidth );
        }
        public void SetCameraY( float cam_y )
        {
            Camera.Y = Math.Clamp( cam_y, 0, LineHeight * Lines.Count * .95f );
        }

        public int GetLineX( float x ) => (int) Math.Floor( ( x - ( CounterBorderSpace + CounterWidth ) ) / GetCursorCharWide() - .5f );
        public int GetLineY( float y ) => (int) Math.Floor( y / LineHeight - .5f ) - 1;

        public override void MousePressed( float x, float y, int button, bool is_touch )
        {
            y = y - Bounds.Y;
            x = x - Bounds.X;

            SetCursorPos( GetLineX( Camera.X + x ), GetLineY( Camera.Y + y ) );
        }

        public override void WheelMoved( int x, int y )
        {
            var speed = -y * 50;

            if ( Keyboard.IsDown( KeyConstant.LCtrl ) )
            {
                if ( TextFont.Size + y < 8 || TextFont.Size + y > 35 ) return;
                TextFont.Derive( TextFont.Size + y );
                ComputeFontHeight();
            }
            else
            {
                //  > Scroll X
                if ( Keyboard.IsDown( KeyConstant.LShift ) )
                    SetCameraX( Camera.X + speed );
                //Camera.X = Math.Clamp( Camera.X + speed, 0, Lines.Aggregate( 0, ( acc, x ) => Math.Max( TextFont.GetWidth( x ), acc ) ) * .75f - CounterWidth );
                //  > Scroll Y
                else
                    Camera.Y = Math.Clamp( Camera.Y + speed, 0, LineHeight * Lines.Count * .95f );
            }
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
                //  > Paste
                else if ( Keyboard.IsDown( KeyConstant.V ) )
                {
                    string text = Clipboard.GetText();
                    Append( text );

                    if ( !text.Contains( "\n" ) )
                        MoveCursorTowards( text.Length, 0 );
                }
            }
            //  > Alt Keys
            else if ( Keyboard.IsDown( KeyConstant.LAlt ) )
            {
                //  > Swap lines
                if ( Keyboard.IsDown( KeyConstant.Up ) )
                    SwapLine( -1 );
                else if ( Keyboard.IsDown( KeyConstant.Down ) )
                    SwapLine( 1 );
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
                    //  > Run
                    case KeyConstant.F5:
                        if ( !Children.Contains( RunButton ) ) return;
                        Save();
                        Run();
                        break;
                }
        }

        public override void TextInput( string text )
        {
            Append( text );
            SetCursorPos( Cursor.X + 1, Cursor.Y );
            //Console.WriteLine( "insert '{0}'", text );
        }

        public override void InnerRender()
        {
            //  > Text
            Graphics.Translate( 0, TitleHeight / 4 );
            Graphics.SetFont( TextFont.LFont );

            current_color = TextColor;
            for ( int i = Math.Max( 0, GetLineY( Camera.Y + TitleHeight ) ); i < Math.Min( Lines.Count, GetLineY( Camera.Y + Bounds.Height + TitleHeight ) ); i++ )
            {
                //  > Line
                int off_x = 0;
                MatchCollection matches = Regex.Matches( Lines[i], Language.Highlighter.WordPattern );
                for ( int u = 0; u < matches.Count; u++ )
                {
                    Match match = matches[u];
                    string word = match.Value;

                    Graphics.SetColor( GetWordColor( word, u == 0/*, u + 1 < matches.Count ? matches[u + 1].Value : ""*/ ) );
                    Graphics.Print( word, (int) ( CounterWidth + CounterBorderSpace - Camera.X + off_x ), (int) ( LineHeight * i - Camera.Y ) );
                    off_x += TextFont.LFont.GetWidth( word );
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
