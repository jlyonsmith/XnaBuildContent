using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class Texture2DContent : TextureContent
    {
        public Texture2DContent(BitmapContent bitmapContent) : base(new MipmapChainCollection(1))
        {
			base.Faces[0] = bitmapContent;
        }

        public Texture2DContent(MipmapChain mipmaps) : base(new MipmapChainCollection(1))
        {
			base.Faces[0] = mipmaps;
        }

        public MipmapChain Mipmaps
        {
            get
            {
                return base.Faces[0];
            }
        }
    }
}

