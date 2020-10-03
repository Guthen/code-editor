using System;
using System.Collections.Generic;
using Love;
using System.IO;
using File = System.IO.File;
using Newtonsoft.Json;

namespace CodeEditor
{
    struct HighlighterTheme
    {
        public Color Number;
        public Color String;
        public Color Syntax;
        public Color Comment;
    }

    class HighlighterParser
    {
        public string[] Syntax;
        public string[] String;
        public string[] Comment;
        public string[] Bool;

        public static Dictionary<string, HighlighterParser> Parsers = new Dictionary<string, HighlighterParser>();
        public static void Load( string path )
        {
            foreach ( string file in Directory.EnumerateFiles( path ) )
            {
                var key = Path.GetFileNameWithoutExtension( file );
                var json = File.ReadAllText( file );
                var value = JsonConvert.DeserializeObject<HighlighterParser>( json );

                Parsers.Add( key, value );
                Console.WriteLine( "Highlighter: load '{0}' for '{1}'", file, key );
            }
        }

        public static HighlighterParser Get( string key ) => Parsers.ContainsKey( key ) ? Parsers[key] : null;
    }
}
