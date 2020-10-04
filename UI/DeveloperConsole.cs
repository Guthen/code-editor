using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Media3D;
using Love;
using File = System.IO.File;

namespace CodeEditor.UI
{
    class DeveloperConsole : Window
    {
        public int CursorX = 0;

        public List<string> Lines = new List<string>();
        public Dictionary<string, Func<DeveloperConsole, List<string>, string>> Commands = new Dictionary<string, Func<DeveloperConsole, List<string>, string>>()
        {
            { 
                "cls", 
                ( DeveloperConsole self, List<string> args ) =>
                {
                    self.Lines.Clear();
                    self.Camera.X = 0;
                    self.Camera.Y = 0;
                    return "";
                } 
            },
            {
                "print",
                ( DeveloperConsole self, List<string> args ) =>
                {
                    return string.Join( " ", args );
                }
            },
            { 
                "theme",
                ( DeveloperConsole self, List<string> args ) =>
                {
                    if ( args.Count < 1 || Theme.Get( args[0] ) == null )
                        return "ERROR: theme doesn't exists";

                    Main.SetTheme( Theme.Get( args[0] ) );
                    Program.Preferences.Theme = args[0];
                    Program.Preferences.Save();
                    return string.Format( "Theme: set to '{0}'", args[0] );
                }
            },
            {
                "cd",
                ( DeveloperConsole self, List<string> args ) =>
                {
                    if ( args.Count < 1 || !Directory.Exists( args[0] ) )
                        return "ERROR: path doesn't exists";

                    self.CurrentDirectory = args[0];
                    return string.Format( "Console: move to '{0}'", args[0] );
                }
            },
            {
                "file",
                ( DeveloperConsole self, List<string> args ) =>
                {
                    //  > Check Variables
                    if ( args.Count < 2 )
                        return "ERROR: must have 2 arguments : 'file <id> <path>'";

                    if ( !int.TryParse( args[0], out int id ) )
                        return "ERROR: failed to parse 'id'";

                    var text_editors = Elements.elements.Where( ( Element el ) => el is TextEditor ).ToList();
                    if ( id >= text_editors.Count || id < 0 )
                        return "ERROR: failed to get element";

                    //  > Get Element
                    var text_editor = (TextEditor) text_editors[id];

                    //  > Get Path
                    var path = "";
                    if ( Regex.Match( args[1], @"^\w:(\/|\\)" ).Success )
                        path = args[1];
                    else
                        path = Path.Combine( self.CurrentDirectory, args[1] );

                    //  > Set File
                    if ( !File.Exists( path ) )
                        return string.Format( "ERROR: '{0}' doesn't exist", path );

                    text_editor.SetFile( path );
                    return "";
                }
            },
        };

        public string PromptCommand = "";
        public string CurrentDirectory = "";

        public DeveloperConsole()
        {
            Title = "Developper Console";
            SetRightTitle( () =>
            {
                return Timer.GetFPS().ToString() + " FPS";
            } );
        }

        public void OutputHandler( object process, DataReceivedEventArgs outline ) => Append( outline.Data );
        public void Append( string text )
        {
            if ( text == null ) return;
            Lines.Add( text );

            var line_total_height = Lines.Count * LineHeight;
            var view = Bounds.Height + Camera.Y - TitleHeight * 3;
            if ( line_total_height > view )
                Camera.Y = line_total_height - TitleHeight * 2;
        }

        public void Prompt( string prompt )
        {
            List<string> args = prompt.Split( " " ).ToList();

            string cmd = args[0];
            args.RemoveAt( 0 );

            Append( "> " + prompt );
            if ( Commands.ContainsKey( cmd ) )
            {
                string output = Commands[cmd]( this, args );

                if ( output.Length > 0 )
                    Append( output );
            }
            else
                Append( "ERROR: Command doesn't exists" );
        }

        public Color GetLineColor( string line )
        {
            if ( line.StartsWith( '>' ) )
                return Main.CurrentTheme.Window.DiscretColor;
            if ( line.StartsWith( "ERROR" ) )
                return Main.CurrentTheme.Highlighter.Syntax;

            return TextColor;
        }

        public int GetClampedCursorX( int x ) => Math.Clamp( x, 0, Math.Max( PromptCommand.Length, 0 ) );
        public int GetCursorX() => TextFont.GetWidth( "> " + PromptCommand.Substring( 0, CursorX ) );
        public int GetCursorCharWide()
        {
            if ( CursorX >= PromptCommand.Length )
                return TextFont.GetWidth( " " );

            string cursor_char = PromptCommand[CursorX].ToString();
            return TextFont.GetWidth( cursor_char );
        }

        public override void TextInput( string text )
        {
            PromptCommand = PromptCommand.Insert( CursorX, text );
            CursorX++;
        }

        public override void KeyPressed( KeyConstant key, Scancode scancode, bool is_repeat )
        {
            if ( Keyboard.IsDown( KeyConstant.LCtrl ) )
            {
                if ( Keyboard.IsDown( KeyConstant.V ) )
                {
                    string text = Clipboard.GetText();
                    PromptCommand = PromptCommand.Insert( CursorX, text );
                    CursorX = GetClampedCursorX( CursorX + text.Length );
                }

                return;
            }

            switch ( key )
            {
                case KeyConstant.Backspace:
                    if ( CursorX > 0 )
                    {
                        PromptCommand = PromptCommand.Remove( CursorX - 1, 1 );
                        CursorX--;
                    }
                    break;
                case KeyConstant.Delete:
                    if ( CursorX < PromptCommand.Length )
                        PromptCommand = PromptCommand.Remove( CursorX, 1 );
                    break;
                case KeyConstant.Left:
                    CursorX = GetClampedCursorX( CursorX - 1 );
                    break;
                case KeyConstant.Right:
                    CursorX = GetClampedCursorX( CursorX + 1 );
                    break;
                case KeyConstant.Enter:
                    Prompt( PromptCommand );

                    PromptCommand = "";
                    CursorX = 0;
                    break;
            }
        }

        public override void WheelMoved( int x, int y )
        {
            var speed = -y * 50;
            if ( Keyboard.IsDown( KeyConstant.LShift ) )
                Camera.X = Math.Clamp( Camera.X + speed, 0, Lines.Aggregate( 0, ( acc, x ) => Math.Max( TextFont.GetWidth( x ), acc ) ) * .75f );
            else
                Camera.Y = Math.Clamp( Camera.Y + speed, 0, LineHeight * Lines.Count * .75f );
        }

        public override void InnerRender()
        {
            //  > Lines
            Graphics.SetFont( TextFont );
            for ( int i = 0; i < Lines.Count; i++ )
            {
                Graphics.SetColor( GetLineColor( Lines[i] ) );
                Graphics.Print( Lines[i], (int) ( Padding.X - Camera.X ), (int) ( i * LineHeight - Camera.Y ) );
            }

            //  > Prompt
            float y = Bounds.Height - TitleHeight - LineHeight * 1.5f - Padding.Z * 2;
            Graphics.SetColor( BackgroundColor );
            Graphics.Rectangle( DrawMode.Fill, 0, y, Bounds.Width - Padding.W * 2, LineHeight * 1.5f );

            Graphics.SetColor( DiscretColor );
            Graphics.Line( 0, y, Padding.X + Bounds.Width - Padding.X * 2, y );

            Graphics.SetColor( TextColor );
            Graphics.Print( "> " + PromptCommand, (int) -Camera.X, y + Padding.Y );

            //  > Cursor
            if ( !IsFocus ) return;
            Graphics.SetColor( TextColor.r, TextColor.g, TextColor.b, .5f );
            Graphics.Rectangle( Timer.GetTime() % 1 <= .5 ? DrawMode.Fill : DrawMode.Line, GetCursorX() - Camera.X, y + Padding.Y, GetCursorCharWide(), LineHeight );
        }
    }
}
