using Love;
using System;
using System.Collections.Generic;
using System.Reflection;
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
                Type type = element.GetType();
                MethodInfo method = type.GetMethod( name );
                method.Invoke( element, args );
            }
        }

        public static void CallFocus( string name, params object[] args )
        {
            if ( Focused == null )
                return;

            Type type = elements[0].GetType();
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
        public Rectangle Bounds { get; set; } = new Rectangle( 0, 0, 450, 300 );
        public bool IsFocus = false;
        public int ID = 0;

        public Element()
        {
            Elements.elements.Add( this );
            ID = Elements.elements.Count;
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

        public void SetSize( int? w = null, int? h = null )
        {
            Rectangle bounds = Bounds;

            if ( !( w == null ) )
                bounds.Width = (int) w;
            if ( !( h == null ) )
                bounds.Height = (int) h;

            Bounds = bounds;
        }

        public bool Intersect( int x, int y, int w = 1, int h = 1 )
        {
            return Bounds.X < x + w && Bounds.Y < y + h
                && x < Bounds.X + Bounds.Width && y < Bounds.Y + Bounds.Height;
        }

        public virtual void Update( float dt ) {}
        public virtual void Render() { }

        public virtual void WheelMoved( int x, int y ) { }
        public virtual void TextInput( string text ) { }
        public virtual void KeyPressed( KeyConstant key, Scancode scancode, bool is_repeat ) { }
    }
}
