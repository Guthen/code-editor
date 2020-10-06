using System;
using System.Collections.Generic;
using System.IO;
using File = System.IO.File;
using Love;
using Newtonsoft.Json;

#pragma warning disable CS0649
namespace CodeEditor
{
    struct HighlighterTheme
    {
        public Color Number;
        public Color String;
        public Color Syntax;
        public Color Comment;
    }

    struct WindowTheme
    {
        public Color TextColor;
        public Color BorderColor;
        public Color BackgroundColor;
        public Color TitleBackgroundColor;
        public Color DiscretColor;
    }

    class Theme
    {
        [JsonProperty( "Highlight" )]
        public HighlighterTheme Highlighter;

        [JsonProperty( "Window" )]
        public WindowTheme Window;

        public static Dictionary<string, Theme> Themes = new Dictionary<string, Theme>();
        public static void Load( string path )
        {
            foreach ( string file in Directory.EnumerateFiles( path ) )
            {
                var key = Path.GetFileNameWithoutExtension( file );
                var json = File.ReadAllText( file );
                var value = JsonConvert.DeserializeObject<Theme>( json );

                Themes.Add( key, value );
                Boot.Log( string.Format( "Theme: load '{0}' for '{1}'", file, key ) );
            }
        }

        public static Theme Get( string key ) => Themes.ContainsKey( key ) ? Themes[key] : null;
    }
}
