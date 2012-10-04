using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using ToolBelt;


namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class BitmapContent
    {
		public int Height { get; private set; }
		public int Width { get; private set; }
		public SurfaceFormat Format { get; private set; }
		public byte[] Data { get; private set; }

		public BitmapContent(SurfaceFormat format, int width, int height, byte[] data)
		{
			this.Format = format;
			this.Width = width;
			this.Height	= height;
			this.Data = data;
		}

        public override string ToString()
        {
			return "{0}, {1}x{2} {3}".InvariantFormat(base.GetType().Name, this.Width, this.Height, this.Format);
        }
    }
}

