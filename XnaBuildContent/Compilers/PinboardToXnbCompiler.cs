using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt; 
using XnaBuildContent;
using Microsoft.Xna.Framework;
using System.IO;

namespace XnaBuildContent.Compilers
{
    public class PinboardToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".pinboard" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            ParsedPath pinboardFileName = Target.InputFiles.Where(f => f.Extension == ".pinboard").First();
            ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();
            PinboardFileV1 pinboard = PinboardFileCache.Load(pinboardFileName);
            Rectangle[] rectangles = new Rectangle[pinboard.RectInfos.Count + 1];

            rectangles[0] = new Rectangle(pinboard.ScreenRectInfo.X, pinboard.ScreenRectInfo.Y, pinboard.ScreenRectInfo.Width, pinboard.ScreenRectInfo.Height);

            for (int i = 0; i < pinboard.RectInfos.Count; i++)
            {
                rectangles[i + 1] = new Rectangle(pinboard.RectInfos[i].X, pinboard.RectInfos[i].Y, pinboard.RectInfos[i].Width, pinboard.RectInfos[i].Height);
            }

			if (!Directory.Exists(xnbFileName.VolumeAndDirectory))
			{
				Directory.CreateDirectory(xnbFileName.VolumeAndDirectory);
			}

            XnbFileWriterV5.WriteFile(rectangles, xnbFileName);
        }

        #endregion
    }
}
