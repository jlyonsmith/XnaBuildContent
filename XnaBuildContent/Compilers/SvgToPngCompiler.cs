using System;
using System.Collections.Generic;
using ToolBelt; 
using XnaBuildContent;
using System.Linq;
using System.IO;

namespace XnaBuildContent.Compilers
{
	public class SvgToPngCompiler : IContentCompiler
	{
		#region Construction
		public SvgToPngCompiler()
		{
		}
		#endregion

		#region IContentCompiler implementation

		public string[] InputExtensions	{ get { return new string[] { ".svg" }; } }
		public string[] OutputExtensions { get { return new string[] { ".png" }; } }
		public BuildContext Context { get; set; }
		public BuildTarget Target { get; set; }

		public void Compile()
		{
			ParsedPath svgFileName = Target.InputFiles.Where(f => f.Extension == ".svg").First();
			ParsedPath pngFileName = Target.OutputFiles.Where(f => f.Extension == ".png").First();

			int width, height;

			Target.Properties.GetRequiredValue("Width", out width);
			Target.Properties.GetRequiredValue("Height", out height);

			if (!Directory.Exists(pngFileName.VolumeAndDirectory))
			{
				Directory.CreateDirectory(pngFileName.VolumeAndDirectory);
			}

			ImageTools.SvgToPngWithInkscape(svgFileName, pngFileName, width, height);
		}

		#endregion
	}
}

