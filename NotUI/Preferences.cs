using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CodeEditor.NotUI
{
    struct Interpreters
    {
        [JsonProperty( "py" )]
        public string Python;
    }

    class Preferences
    {
        public int WindowWidth;
        public int WindowHeight;
        public bool WindowFullscreen;
        public Interpreters Interpreters;

        public string Theme;

        public void Save()
        {
            File.WriteAllText( "Assets/preferences.json", JsonConvert.SerializeObject( this, Formatting.Indented ) );
        }
    }
}
