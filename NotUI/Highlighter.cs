using System;
using System.Collections.Generic;
using Love;
using System.IO;
using File = System.IO.File;
using Newtonsoft.Json;

#pragma warning disable CS0649
namespace CodeEditor
{
    struct MultilineComment
    {
        public string[] Start;
        public string[] End;
    }

    class HighlighterParser
    {
        public string[] Syntax;
        public string[] String;
        public string[] Comment;
        public string[] Bool;
        public string WordPattern = @"\w+|\\\W|\W";
        public MultilineComment MultilineComment = new MultilineComment()
        {
            Start = new string[] { },
            End = new string[] { },
        };

        public bool Success = true;

        public static Dictionary<string, HighlighterParser> Parsers = new Dictionary<string, HighlighterParser>();
        public static void Load( string path )
        {
            foreach ( string file in Directory.EnumerateFiles( path ) )
            {
                var key = Path.GetFileNameWithoutExtension( file );
                var json = File.ReadAllText( file );
                var value = JsonConvert.DeserializeObject<HighlighterParser>( json );

                Parsers.Add( key, value );
                Boot.Log( string.Format( "Highlighter: load '{0}' for '{1}'", file, key ) );
            }
        }

        public static HighlighterParser Get( string key ) => Parsers.ContainsKey( key ) ? Parsers[key] : null;
    }
}
