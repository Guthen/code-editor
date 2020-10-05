using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

#pragma warning disable CS0649
namespace CodeEditor.NotUI
{
    class Preferences
    {
        public int WindowWidth;
        public int WindowHeight;
        public bool WindowFullscreen;
        public Dictionary<string, string> Interpreters;

        public string Theme;

        public void Save()
        {
            File.WriteAllText( "Assets/preferences.json", JsonConvert.SerializeObject( this, Formatting.Indented ) );
        }
    }
}
