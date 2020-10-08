using Love;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CodeEditor.NotUI
{
    class Font
    {
        public string Path;
        public int Size;
        public Love.Font LFont;

        public Font( string path, int size )
        {
            Path = path;
            Size = size;
            LFont = Graphics.NewFont( path, size );
        }

        public Font Copy() => new Font( Path, Size );

        public void Derive( int new_size )
        {
            Size = new_size;
            LFont.Dispose();
            LFont = Graphics.NewFont( Path, Size );
        }
    }
}
