using System;
using System.Collections.Generic;
using System.Text;
using Love;

namespace CodeEditor.UI
{
    class DeveloperConsole : Window
    {
        public List<string> Lines = new List<string>()
        {
            "YOOOO",
            "gayyy",
            "na",
            "tg"
        };

        public DeveloperConsole()
        {
            Title = "Developper Console";
            SetRightTitle( () =>
            {
                return Timer.GetFPS().ToString() + " FPS";
            } );
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
            Graphics.SetColor( DiscretColor );

            float y = Bounds.Height - TitleHeight - LineHeight * 1.5f - Padding.Z;
            Graphics.Line( 0, y, Padding.X + Bounds.Width - Padding.X * 2, y );
        }
    }
}
