using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ToolBelt;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class TextureContent
    {
        private MipmapChainCollection faces;

        protected TextureContent(MipmapChainCollection faces)
        {
            this.faces = faces;
        }

        public MipmapChainCollection Faces
        {
            get
            {
                return this.faces;
            }
        }
    }
}

