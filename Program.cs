using Love;
using System;

namespace CodeEditor
{
    class Program
    {
        static void Main( string[] args )
        {
            Boot.Run( new Main(), new BootConfig()
            {
                WindowCentered = true,
                WindowDisplay = 0,
                WindowResizable = true,
            } );
        }
    }
}
