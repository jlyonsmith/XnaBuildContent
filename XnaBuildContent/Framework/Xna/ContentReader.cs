using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Microsoft.Xna.Framework.Content
{
    public class ContentReader : BinaryReader
    {
        public ContentReader(Stream stream) : base(stream)
        {
        }
    }
}

