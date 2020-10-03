using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Love;

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
                "file",
                ( DeveloperConsole self, List<string> args ) =>
                {
                    //  > Check variables
                    if ( !int.TryParse( args[0], out int id ) )
                        return "ERROR: failed to parse 'id'";

                    if ( id >= Elements.elements.Count || id < 0 )
                        return "ERROR: failed to get element";

                    //  > Get element
                    var element = Elements.elements[id];
                    if ( !( element is TextEditor ) )
                        return "ERROR: element is not a TextEditor";

                    var path = @"D:\Projets\Lua\Car2Game\classic.lua";
                    ( (TextEditor) element ).SetFile( path );
                    return "File changed";
                }
            },
        };

        public string PromptCommand = "";

        public DeveloperConsole()
        {
            Title = "Developper Console";
            SetRightTitle( () =>
            {
                return Timer.GetFPS().ToString() + " FPS";
            } );
        }

        public void Append( string text ) => Lines.Add( text );

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
            Camera.Y = Math.Clamp( Camera.Y - y * 50, 0, LineHeight * Lines.Count * .75f );
        }

        public override void InnerRender()
        {
            //  > Lines
            Graphics.SetFont( TextFont );
            Graphics.SetColor( TextColor );
            for ( int i = 0; i < Lines.Count; i++ )
            {
                Graphics.Print( Lines[i], Padding.X - Camera.X, i * LineHeight - Camera.Y );
            }

            //  > Prompt
            float y = Bounds.Height - TitleHeight - LineHeight * 1.5f - Padding.Z * 2;
            Graphics.SetColor( BackgroundColor );
            Graphics.Rectangle( DrawMode.Fill, 0, y, Bounds.Width - Padding.W * 2, LineHeight * 1.5f );

            Graphics.SetColor( DiscretColor );
            Graphics.Line( 0, y, Padding.X + Bounds.Width - Padding.X * 2, y );

            Graphics.SetColor( TextColor );
            Graphics.Print( "> " + PromptCommand, 0, y + Padding.Y );

            //  > Cursor
            if ( !IsFocus ) return;
            Graphics.SetColor( TextColor.r, TextColor.g, TextColor.b, .5f );
            Graphics.Rectangle( Timer.GetTime() % 1 <= .5 ? DrawMode.Fill : DrawMode.Line, GetCursorX() - Camera.X, y + Padding.Y, GetCursorCharWide(), LineHeight );
        }
    }
}
