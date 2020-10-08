using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

#pragma warning disable CS0649
namespace CodeEditor.NotUI
{
    class Preferences
    {
        public int WindowWidth = 1080;
        public int WindowHeight = 720;
        public bool WindowFullscreen = false;
        public Dictionary<string, Dictionary<string, string>> Interpreters = new Dictionary<string, Dictionary<string, string>>();

        public bool Success = true;
        public string Theme = "default";
        public static string Path = "Assets/preferences.json";

        public void Save()
        {
            File.WriteAllText( Path, JsonConvert.SerializeObject( this, Formatting.Indented ) );
        }
    }
}
