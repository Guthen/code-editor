using System;
using System.Collections.Generic;
using System.Text;
using Love;
using CodeEditor.UI;
using System.IO;
using Window = CodeEditor.UI.Window;

namespace CodeEditor
{
    class Main : Scene
    {
        public override void Load()
        {
            Keyboard.SetKeyRepeat( true );

            Font TextFont = Graphics.NewFont( "Assets/consola.ttf", 15 );
            Font TitleFont = Graphics.NewFont( "Assets/consolab.ttf", 16 );

            //  > Coolors: https://coolors.co/090c08-fff7f8-696d7d-ffe74c-ff5964-6369d1-6cae75-8b9474
            HighlighterTheme highlighter = new HighlighterTheme()
            {
                String = new Color( 255, 231, 76, 255 ),
                Number = new Color( 146, 213, 230, 255 ),
                Comment = new Color( 108, 174, 117, 255 ),
                Syntax = new Color( 255, 89, 100, 255 ),
            };
            HighlighterParser.Load( "Highlighters" );

            var te_2 = new TextEditor();
            te_2.SetFont( TextFont, TitleFont );
            te_2.SetFile( @"K:\Projets\Lua\Löve2D\DungeonDemons\main.lua" );
            te_2.SetSize( 500, Graphics.GetHeight() );
            te_2.HighlighterTheme = highlighter;

            var te_1 = new TextEditor();
            te_1.SetFont( TextFont, TitleFont );
            te_1.SetFile( @"K:\Programmes\Steam\steamapps\common\GarrysMod\garrysmod\addons\GNLib\addon.json" );
            te_1.SetPos( te_2.Bounds.Width );
            te_1.SetSize( Graphics.GetWidth() - te_2.Bounds.Width, 400 );
            te_1.HighlighterTheme = highlighter;

            //var console = new DeveloperConsole();
            //console.SetFont( TextFont, TitleFont );
            //console.SetPos( te_1.Bounds.X, te_1.Bounds.Y + te_1.Bounds.Height );
            //console.SetSize( te_1.Bounds.Width, Graphics.GetHeight() - te_1.Bounds.Height );

            var te_3 = new TextEditor();
            te_3.SetFont( TextFont, TitleFont );
            te_3.SetFile( @"K:\Projets\Python\py-icewalker\main_1.py" );
            te_3.SetPos( te_1.Bounds.X, te_1.Bounds.Y + te_1.Bounds.Height );
            te_3.SetSize( te_1.Bounds.Width, Graphics.GetHeight() - te_1.Bounds.Height );
            te_3.HighlighterTheme = highlighter;

            Elements.Focus( te_2 );
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

        public override void MousePressed( float x, float y, int button, bool isTouch )
        {
            var element = Elements.GetElementAt( (int) x, (int) y );
            if ( !( element == null ) )
                Elements.Focus( element );
        }

        public override void Draw() => Elements.Call( "Render" );
    }
}
