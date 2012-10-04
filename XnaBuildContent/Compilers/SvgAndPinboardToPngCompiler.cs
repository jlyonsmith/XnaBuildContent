using System;
using ToolBelt; 
using XnaBuildContent;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using System.IO;

namespace XnaBuildContent.Compilers
{
	public class SvgAndPinboardToPngCompiler : IContentCompiler
	{
		#region IContentCompiler implementation

		public string[] InputExtensions { get { return new string[] { ".svg", ".pinboard" }; } }
		public string[] OutputExtensions { get { return new string[] { ".png" }; } }
		public BuildContext Context { get; set; }
		public BuildTarget Target { get; set; }

		public void Compile()
		{
			IEnumerable<ParsedPath> svgPaths = Target.InputFiles.Where(f => f.Extension == ".svg");
			ParsedPath pinboardPath = Target.InputFiles.Where(f => f.Extension == ".pinboard").First();
			ParsedPath pngPath = Target.OutputFiles.Where(f => f.Extension == ".png").First();
			PinboardFileV1 pinboardFile = PinboardFileCache.Load(pinboardPath);
			List<ImagePlacement> placements = new List<ImagePlacement>();
			string[] rectangleNames;

			Target.Properties.GetRequiredValue("Rectangles", out rectangleNames);

			if (svgPaths.Count() != rectangleNames.Length)
				throw new ContentFileException("Number of .svg files ({0}) does match number of RectangleNames ({1})"
					.CultureFormat(svgPaths.Count(), rectangleNames.Length));

			string s = Target.Properties.GetOptionalValue("Rotation", "None");

			ImageRotation rotation;

			if (!Enum.TryParse(s, out rotation))
				throw new ContentFileException("Invalid value '{0}' for given for rotation.  Valid are None, Left, Right, UpsideDown".CultureFormat(s));

			int i = 0;

			try
			{
				if (!Directory.Exists(pngPath.VolumeAndDirectory))
				{
					Directory.CreateDirectory(pngPath.VolumeAndDirectory);
				}

				foreach (var svgPath in svgPaths)
				{
					PinboardFileV1.RectangleInfo rectInfo = pinboardFile.GetRectangleInfoByName(rectangleNames[i]);
					ParsedPath tempPngPath = pngPath.WithFileAndExtension(String.Format("{0}_{1}.png", pngPath.File, i));
					
					if (rectInfo == null)
					{
						throw new ContentFileException("Rectangle '{0}' not found in pinboard file '{1}'"
	                    	.CultureFormat(rectangleNames[i], pinboardFile)); 
					}

					ImageTools.SvgToPngWithInkscape(svgPath, tempPngPath, rectInfo.Width, rectInfo.Height);

					placements.Add(new ImagePlacement(
						tempPngPath, new Cairo.Rectangle(rectInfo.X, rectInfo.Y, rectInfo.Width, rectInfo.Height)));

					i++;
				}

				ImageTools.CombinePngs(placements, pngPath);
				ImageTools.RotatePng(pngPath, rotation);
			}
			finally
			{
				foreach (var placement in placements)
				{
					if (File.Exists(placement.ImageFile))
						File.Delete(placement.ImageFile);
				}
			}
		}

		#endregion
	}
}

