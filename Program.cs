using CodeEditor.NotUI;
using Love;
using Newtonsoft.Json;
using System;
using File = System.IO.File;

namespace CodeEditor
{
    class Program
    {
        public static Preferences Preferences;

        [STAThread]
        static void Main( string[] args )
        {
            Preferences = JsonConvert.DeserializeObject<Preferences>( File.ReadAllText( "Assets/preferences.json" ) );

            Boot.Run( new Main(), new BootConfig()
            {
                WindowCentered = true,
                WindowDisplay = 0,
                WindowResizable = true,
                WindowFullscreen = Preferences.WindowFullscreen,
                WindowWidth = Preferences.WindowWidth,
                WindowHeight = Preferences.WindowHeight,
            } );
        }
    }
}
