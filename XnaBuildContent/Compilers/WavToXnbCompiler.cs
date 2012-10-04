using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt; 
using XnaBuildContent;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace XnaBuildContent.Compilers
{
    public class WavToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".wav" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".xnb" }; }
        }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            ParsedPath wavFileName = Target.InputFiles.Where(f => f.Extension == ".wav").First();
            ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();

			WavFile wavFile = WavFileReader.ReadFile(wavFileName);
			AudioContent ac = new AudioContent(wavFile);

			if (!Directory.Exists(xnbFileName.VolumeAndDirectory))
				Directory.CreateDirectory(xnbFileName.VolumeAndDirectory);

            XnbFileWriterV5.WriteFile(ac, xnbFileName);
        }

        #endregion
    }
}
