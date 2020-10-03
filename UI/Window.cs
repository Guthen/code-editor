using Love;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeEditor.UI
{
    class Window : Element
    {
        public Vector2 Camera = new Vector2();
        public Vector4 Padding;

        public int LineHeight;
        public int TitleHeight;
        public int BorderWide = 1;

        public Color TextColor = new Color( 255, 247, 248, 255 );
        public Color BorderColor = new Color( 9, 12, 8, 255 );
        public Color BackgroundColor = new Color( 105, 109, 125, 255 );
        public Color TitleBackgroundColor = new Color( 99, 105, 209, 255 );
        public Color DiscretColor = new Color( 163, 163, 163, 255 );

        public Font TextFont = Graphics.GetFont();
        public Font TitleFont = Graphics.GetFont();

        public string Title = "Untitled";
        public Func<string> RightTitle = () => { return ""; };

        public Window()
        {
            Bounds = new Rectangle( 0, 0, 300, 300 );
            Padding = new Vector4( 4, 4, 4, 4 );

            ComputeFontHeight();
        }

        public void ComputeFontHeight()
        {
            LineHeight = TextFont.GetHeight();
            TitleHeight = TitleFont.GetHeight();
        }

        public void SetFont( Font text, Font title )
        {
            TextFont = text;
            TitleFont = title;

            ComputeFontHeight();
        }

        public void SetRightTitle( Func<string> func ) => RightTitle = func;

        public virtual void InnerRender() {}

        public override void Render()
        {
            float inner_x = Bounds.X + Padding.X;
            float inner_y = Bounds.Y + Padding.Y;

            //  > Background
            Graphics.SetColor( BackgroundColor );
            Graphics.Rectangle( DrawMode.Fill, Bounds );

            //  > Border
            Graphics.SetColor( BorderColor );
            for ( int i = 0; i < BorderWide; i++ )
                Graphics.Rectangle( DrawMode.Line, Bounds.X + i, Bounds.Y + i, Bounds.Width - i * 2, Bounds.Height - i * 2 );

            inner_y += TitleHeight;
            //  > Stencil
            Graphics.Stencil( () =>
            {
                Graphics.SetColor( Color.White );
                Graphics.Rectangle( DrawMode.Fill, new RectangleF( Bounds.X + Padding.X, Bounds.Y + Padding.Y, Bounds.Width - Padding.Z * 2, Bounds.Height - Padding.W * 2 ) );
            }, StencilAction.Replace, ID, true ); //  > keepValue = true is important
            Graphics.SetStencilTest( CompareMode.Equal, ID );

            //  > Inner Render
            inner_y += TitleHeight / 4;
            Graphics.Push();
                Graphics.Translate( inner_x, inner_y );
                InnerRender();
                Graphics.Translate( -inner_x, -inner_y );
            Graphics.Pop();
            inner_y -= TitleHeight / 4;

            Graphics.SetStencilTest();

            //  > Title
            inner_y -= TitleHeight;
            Graphics.SetFont( TitleFont );
            Graphics.SetColor( TitleBackgroundColor );
            Graphics.Rectangle( DrawMode.Fill, new RectangleF( inner_x, inner_y, Bounds.Width - Padding.Z * 2, TitleHeight ) );

            Graphics.SetColor( TextColor );
            Graphics.Print( Title, inner_x + Padding.X, inner_y + 1 );

            var limit = 150;
            Graphics.Printf( RightTitle(), inner_x + Padding.X + Bounds.Width - Padding.Z * 2 - Padding.X * 2 - limit, inner_y + 1, limit, AlignMode.Right );
        }
    }
}
