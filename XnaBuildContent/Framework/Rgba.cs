using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaBuildContent
{
    public struct Rgba
    {
        public Rgba(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }
}

