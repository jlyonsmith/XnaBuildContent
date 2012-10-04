using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt; 
using XnaBuildContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace XnaBuildContent.Compilers.Compilers
{
    class PngToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".png" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }

        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
		{
			ParsedPath pngFileName = Target.InputFiles.Where(f => f.Extension == ".png").First();
			ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();

			string compressionType;
			Texture2DContent textureContent;

			Target.Properties.GetOptionalValue("CompressionType", out compressionType, "None");

			ImageTools.CompressPngToTexture2DContent(pngFileName, compressionType, out textureContent);

            XnbFileWriterV5.WriteFile(textureContent, xnbFileName);
        }

        #endregion
    }
}
