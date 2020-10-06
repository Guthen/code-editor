using CodeEditor.NotUI;
using Love;
using Newtonsoft.Json;
using System;
using System.IO;
using File = System.IO.File;

namespace CodeEditor
{
    class Program
    {
        public static Preferences Preferences;
        public static Boot Boot;

        [STAThread]
        static void Main( string[] args )
        {
            try
            {
                Preferences = JsonConvert.DeserializeObject<Preferences>( File.ReadAllText( Preferences.Path ) );
            }
            catch ( IOException )
            {
                Preferences = new Preferences()
                {
                    Success = false,
                };
            }

            Love.Boot.Run( new Boot(), new BootConfig()
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
