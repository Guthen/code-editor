﻿using System;
using System.Collections.Generic;
using System.Text;
using Love;
using CodeEditor.UI;
using System.IO;
using Window = CodeEditor.UI.Window;
using Newtonsoft.Json;

namespace CodeEditor
{
    class Main : Scene
    {
        //  > Developer Console
        public static DeveloperConsole DevConsole;
        public static void Log( string text )
        {
            Console.WriteLine( text );

            if ( DevConsole == null ) return;
            DevConsole.Append( text );
        }

        //  > Theme
        public static Theme CurrentTheme;
        public static void SetTheme( Theme theme )
        {
            //  > Window's colors
            Window.TextColor = theme.Window.TextColor;
            Window.BorderColor = theme.Window.BorderColor;
            Window.BackgroundColor = theme.Window.BackgroundColor;
            Window.TitleBackgroundColor = theme.Window.TitleBackgroundColor;
            Window.DiscretColor = theme.Window.DiscretColor;

            Graphics.SetBackgroundColor( Window.BackgroundColor );

            CurrentTheme = theme;
        }

        public override void Load()
        {
            Keyboard.SetKeyRepeat( true );

            Font TextFont = Graphics.NewFont( "Assets/Fonts/consola.ttf", 15 );
            Font TitleFont = Graphics.NewFont( "Assets/Fonts/consolab.ttf", 16 );

            DevConsole = new DeveloperConsole();

            //  > Coolors: https://coolors.co/090c08-fff7f8-696d7d-ffe74c-ff5964-6369d1-6cae75-8b9474
            HighlighterParser.Load( "Assets/Highlighters" );

            //  > Elements
            var te_2 = new TextEditor();
            te_2.SetFont( TextFont, TitleFont );
            te_2.SetFile( @"K:\Projets\Python\test.py" );
            te_2.SetSize( (int) ( Graphics.GetWidth() * .65f ), Graphics.GetHeight() );
            //te_2.HighlighterTheme = highlighter;

            var te_1 = new TextEditor();
            te_1.SetFont( TextFont, TitleFont );
            te_1.SetFile( @"K:\Projets\C#\code-editor\Program.cs" );
            te_1.SetPos( te_2.Bounds.Width );
            te_1.SetSize( Graphics.GetWidth() - te_2.Bounds.Width, 400 );
            //te_1.HighlighterTheme = highlighter;

            DevConsole.SetFont( TextFont, TitleFont );
            DevConsole.SetPos( te_1.Bounds.X, te_1.Bounds.Y + te_1.Bounds.Height );
            DevConsole.SetSize( te_1.Bounds.Width, Graphics.GetHeight() - te_1.Bounds.Height );

            //var te_3 = new TextEditor();
            //te_3.SetFont( TextFont, TitleFont );
            //te_3.SetFile( @"K:\Projets\Python\py-icewalker\main_1.py" );
            //te_3.SetPos( te_1.Bounds.X, te_1.Bounds.Y + te_1.Bounds.Height );
            //te_3.SetSize( te_1.Bounds.Width, Graphics.GetHeight() - te_1.Bounds.Height );
            //te_3.HighlighterTheme = highlighter;

            Elements.Focus( DevConsole );

            //  > Themes
            Theme.Load( "Assets/Themes" );
            SetTheme( Theme.Get( Program.Preferences.Theme ) );
        }

        public override void Update( float dt ) => Elements.Call( "Update", dt );

        public override void WheelMoved( int x, int y )
        {
            var element = Elements.GetElementAt( (int) Mouse.GetX(), (int) Mouse.GetY() );
            if ( !( element == null ) )
                element.WheelMoved( x, y );
        }

        public override void KeyPressed( KeyConstant key, Scancode scancode, bool is_repeat ) => Elements.CallFocus( "KeyPressed", key, scancode, is_repeat );
        public override void TextInput( string text ) => Elements.CallFocus( "TextInput", text );

        public override void MousePressed( float x, float y, int button, bool is_touch )
        {
            //  > Call
            for ( int i = 0; i < Elements.elements.Count; i++ )
            {
                Element el = Elements.elements[i];
                if ( el.Intersect( (int) x, (int) y ) )
                    el.MousePressed( x, y, button, is_touch );
            }

            //  > Focus
            var element = Elements.GetElementAt( (int) x, (int) y );
            if ( element == null ) return;

            if ( element is Window )
                Elements.Focus( element );
        }

        public override void Draw() => Elements.Call( "Render" );
    }
}
