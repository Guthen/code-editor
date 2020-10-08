using System;
using System.Collections.Generic;
using Love;
using System.IO;
using File = System.IO.File;
using Newtonsoft.Json;
using System.Linq;

#pragma warning disable CS0649
namespace CodeEditor
{
    struct MultilineComment
    {
        public string[] Start;
        public string[] End;
    }

    class HighlighterParser {
        public string[] Syntax = new string[] { };
        public string[] String = new string[] { };
        public string[] Comment = new string[] { };
        public MultilineComment MultilineComment = new MultilineComment()
        {
            Start = new string[] { },
            End = new string[] { },
        };
        public string[] Bool = new string[] { };
        public string WordPattern = @"\w+|\\\W|\W";
    }

    class Language
    {
        public HighlighterParser Highlighter;
        public Dictionary<string, string> RunCommands;

        public bool Success = true;
        public bool IsSyntax( string word ) => Highlighter.Syntax.Contains( word );
        public bool IsString( string word ) => Highlighter.String.Contains( word );
        public bool IsInlineComment( string word ) => Highlighter.Comment.Contains( word );
        public bool IsStartMultilineComment( string word ) => Highlighter.MultilineComment.Start.Contains( word );
        public bool IsEndMultilineComment( string word ) => Highlighter.MultilineComment.End.Contains( word );
        public bool IsBool( string word ) => Highlighter.Bool.Contains( word );

        public static Dictionary<string, Language> Parsers = new Dictionary<string, Language>();
        public static void Load( string path )
        {
            foreach ( string file in Directory.EnumerateFiles( path ) )
            {
                var key = Path.GetFileNameWithoutExtension( file );
                var json = File.ReadAllText( file );
                var value = JsonConvert.DeserializeObject<Language>( json );

                Parsers.Add( key, value );
                Boot.Log( string.Format( "Highlighter: load '{0}' for '{1}'", file, key ) );
            }
        }

        public static Language Get( string key ) => Parsers.ContainsKey( key ) ? Parsers[key] : null;
    }
}
