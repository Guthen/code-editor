using Love;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace CodeEditor.UI
{
    static class Elements
    {
        public static readonly List<Element> elements = new List<Element>();
        public static Element Focused = null;

        public static void Focus( Element element )
        {
            if ( !( Focused == null ) )
                Focused.IsFocus = false;

            Focused = element;
            Focused.IsFocus = true;
        }

        public static void Call( string name, params object[] args )
        {
            if ( elements.Count <= 0 )
                return;

            foreach ( Element element in elements )
            {
                if ( name == "Render" && !element.Visible ) continue;

                Type type = element.GetType();
                MethodInfo method = type.GetMethod( name );
                method.Invoke( element, args );
            }
        }

        public static void CallFocus( string name, params object[] args )
        {
            if ( Focused == null )
                return;

            Type type = Focused.GetType();
            MethodInfo method = type.GetMethod( name );
            method.Invoke( Focused, args );
        }

        public static Element GetElementAt( int x, int y )
        {
            foreach ( Element element in elements )
                if ( element.Intersect( x, y ) )
                    return element;

            return null;
        }
    }

    class Element
    {
        public Element Parent;
        public List<Element> Children = new List<Element>();

        public RectangleF FractionBounds = new RectangleF();
        public Rectangle Bounds { get; set; } = new Rectangle( 0, 0, 450, 300 );
        public bool IsFocus = false;
        public bool Visible = true;
        public int ID = 0;

        public Element()
        {
            Elements.elements.Add( this );
            ID = Elements.elements.Count;
        }

        public void Destroy()
        {
            foreach ( Element child in Children )
                child.Destroy();

            Elements.elements.Remove( this );
        }

        public void SetPos( int? x = null, int? y = null )
        {
            Rectangle bounds = Bounds;

            if ( !( x == null ) )
                bounds.X = (int) x;
            if ( !( y == null ) )
                bounds.Y = (int) y;

            Bounds = bounds;
        }

        public virtual void ComputeLayout() { }

        public void SetSize( int? w = null, int? h = null )
        {
            Rectangle bounds = Bounds;

            if ( !( w == null ) )
                bounds.Width = (int) w;
            if ( !( h == null ) )
                bounds.Height = (int) h;

            Bounds = bounds;
            ComputeLayout();
        }

        public void ComputeBounds()
        {
            if ( FractionBounds.IsEmpty ) return;

            int w = Graphics.GetWidth();
            int h = Graphics.GetHeight();

            SetPos( (int) ( FractionBounds.X * w ), (int) ( FractionBounds.Y * h ) );
            SetSize( (int) ( FractionBounds.Width * Graphics.GetWidth() ), (int) ( FractionBounds.Height * h ) );
        }
        public void SetFractionSize( float w, float h )
        {
            FractionBounds.Width = w;
            FractionBounds.Height = h;
        }
        public void SetFractionPos( float x, float y )
        {
            FractionBounds.X = x;
            FractionBounds.Y = y;
        }

        public Rectangle GetAbsoluteBounds()
        {
            Rectangle bounds = Bounds;

            //  > Add Parent Bounds
            if ( !( Parent == null ) )
            {
                bounds.X += Parent.Bounds.X;
                bounds.Y += Parent.Bounds.Y;
            }

            return bounds;
        }

        public bool Intersect( int x, int y, int w = 1, int h = 1 )
        {
            Rectangle bounds = GetAbsoluteBounds();
            return bounds.X < x + w && bounds.Y < y + h
                && x < bounds.X + bounds.Width && y < bounds.Y + bounds.Height;
        }

        public virtual void Update( float dt ) {}
        public virtual void Render() { }

        public virtual void WheelMoved( int x, int y ) { }
        public virtual void TextInput( string text ) { }
        public virtual void KeyPressed( KeyConstant key, Scancode scancode, bool is_repeat ) { }
        public virtual void MousePressed( float x, float y, int button, bool is_touch ) { }
    }
}
