using CodeEditor.NotUI;
using Love;
using System;
using Font = CodeEditor.NotUI.Font;
using Timer = CodeEditor.NotUI.Timer;

namespace CodeEditor.UI
{
    class WindowButton : Element
    {
        public Image Image = Graphics.NewImage( "Assets/Images/icons.png" );
        public Quad Quad;

        public Action<WindowButton> Action = ( WindowButton self ) => { Boot.Log( "Clicked" ); };

        public WindowButton( int icon_id, Element parent, Action<WindowButton> action )
        {
            SetSize( 16, 16 );
            SetIconID( icon_id );

            Action = action;
            Parent = parent;
        }

        public void SetIconID( int id )
        {
            Quad = Graphics.NewQuad( id * Image.GetHeight(), 0, Image.GetHeight(), Image.GetHeight(), Image.GetWidth(), Image.GetHeight() );
        }

        public override void MousePressed( float x, float y, int button, bool is_touch ) => Action( this );

        public override void Render()
        {
            //  > Get Position
            var bounds = GetAbsoluteBounds();
            var x = bounds.X;
            var y = bounds.Y;

            //  > Draw
            Graphics.SetColor( Intersect( (int) Mouse.GetX(), (int) Mouse.GetY() ) ? new Color( Window.TextColor.r, Window.TextColor.g, Window.TextColor.b, 175 ) : Window.TextColor );
            if ( !( Image == null ) )
            {
                if ( !( Quad == null ) )
                {
                    var viewport = Quad.GetViewport();
                    Graphics.Draw( Quad, Image, x, y, 0, viewport.Width / bounds.Width, viewport.Height / bounds.Height );
                }
                else
                    Graphics.Draw( Image, x, y, 0, Image.GetWidth() / bounds.Width, Image.GetHeight() / bounds.Height );
            }
            else
                Graphics.Rectangle( DrawMode.Fill, x, y, Bounds.Width, Bounds.Height );
        }
    }

    class Window : Element
    {
        public Vector2 Camera = new Vector2();
        public Vector4 Padding;

        public int LineHeight;
        public int TitleHeight;
        public int BorderWide = 1;

        public static Color TextColor = new Color( 255, 247, 248, 255 );
        public static Color BorderColor = new Color( 9, 12, 8, 255 );
        public static Color BackgroundColor = new Color( 105, 109, 125, 255 );
        public static Color TitleBackgroundColor = new Color( 99, 105, 209, 255 );
        public static Color DiscretColor = new Color( 163, 163, 163, 255 );

        public Font TextFont;
        public Font TitleFont;

        public string Title = "Untitled";
        public Func<string> RightTitle = () => { return ""; };

        public Window()
        {
            Bounds = new Rectangle( 0, 0, 300, 300 );
            Padding = new Vector4( 4, 4, 4, 4 );

            //ComputeFontHeight();

            Children.Add( new WindowButton( 0, this, ( WindowButton self ) => self.Parent.Destroy() ) );
        }

        //public void AddButton( WindowButton button )
        //{
        //    Children.Add( button );

        //    new Timer( "ComputeLayout_" + ID.ToString(), .01f, () => ComputeLayout() );
        //}

        public override void ComputeLayout()
        {
            //  > Place Children (Buttons)
            for ( int i = 0; i < Children.Count; i++ )
            {
                Element child = Children[i];
                child.SetPos( Bounds.Width - (int) ( Padding.Z * 1.5f ) - child.Bounds.Width * ( i + 1 ), (int) ( Padding.Y * 1.25f ) );
            }
        }

        public void ComputeFontHeight()
        {
            if ( TextFont == null ) return;
            LineHeight = TextFont.LFont.GetHeight();
            TitleHeight = TitleFont.LFont.GetHeight();
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
            Graphics.SetFont( TitleFont.LFont );
            Graphics.SetColor( TitleBackgroundColor );
            Graphics.Rectangle( DrawMode.Fill, new RectangleF( inner_x, inner_y, Bounds.Width - Padding.Z * 2, TitleHeight ) );

            Graphics.SetColor( TextColor );
            Graphics.Print( Title, inner_x + Padding.X, inner_y + Padding.Y * .5f );

            var limit = Bounds.Height / 2;
            Graphics.Printf( RightTitle(), inner_x + Padding.X + Bounds.Width - Padding.Z * 2 - Padding.X * 4 - Children.Count * 16 - limit, inner_y + Padding.Y * .5f, limit, AlignMode.Right );
        }
    }
}
