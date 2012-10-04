using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt; 
using XnaBuildContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Cairo;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

namespace XnaBuildContent.Compilers
{
    public class SpritefontAndStringsToXnbCompiler : IContentCompiler
    {
		#region Classes
		class CharacterData
		{
			// The character as a string.  The "toy" Cairo wrapper only supports strings
			public string Character { get; set; }
			// The bearing information from the Cairo TextExtent.  This is a point on the font baseline
			// from which Cairo draws the character; the "origin".  Our origin is at (0,0) so we store 
			// the negative Cairo values.
			public Cairo.PointD Bearing { get; set; }
			// The location of the character in the generated bitmap.  The characters are currently
			// drawn in a horizontal strip with no padding and with their tops aligned.
			public Cairo.Rectangle Location { get; set; }
			// This is the XNA cropping information which has nothing to do with cropping (see Nuclex 
			// project SpriteFontContent.h for more details):
			//  - X/Y is the amount to move the characters bitmap from the "pen", which in the XNA
			//    drawing routines is at the top left corner of the sprite (0,0).
			//  - W is the amount to move the pen in the X coordinate after drawing the character
			//  - H is the line height of the font; the max ascent, max descent and inter line spacing
			public Cairo.Rectangle Cropping { get; set; }
			// This simple ABC spacing information, not real kerning information:
			//  - X is the space before the character bitmap
			//  - Y is the total width of the character bitmap
			//  - Z is the space after the character bitmap
			public Vector3 Kerning { get; set; }
		}
		#endregion

        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".spritefont", ".strings" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
		{
			ParsedPath spriteFontFileName = Target.InputFiles.Where(f => f.Extension == ".spritefont").First();
			ParsedPath stringsFileName = Target.InputFiles.Where(f => f.Extension == ".strings").First();
			ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();
        
			SpriteFontFile sff = SpriteFontFileReader.ReadFile(spriteFontFileName);
			StringsFileV1 sf = StringsFileReaderV1.ReadFile(stringsFileName);

			HashSet<char> hs = new HashSet<char>();

			foreach (var item in sf.Strings)
			{
				for (int i = 0; i < item.Value.Length; i++)
				{
					hs.Add(item.Value[i]);
				}
			}

			foreach (var region in sff.CharacterRegions)
			{
				for (char c = region.Start; c <= region.End; c++)
				{
					hs.Add(c);
				}
			}

			List<char> fontChars = hs.OrderBy(c => c).ToList();
			FontSlant fontSlant = (sff.Style == SpriteFontFile.FontStyle.Italic ? FontSlant.Italic : FontSlant.Normal);
			FontWeight fontWeight = (sff.Style == SpriteFontFile.FontStyle.Bold ? FontWeight.Bold : FontWeight.Normal);
			ParsedPath pngFile = xnbFileName.WithExtension(".png");
			SpriteFontContent sfc = CreateSpriteFontContent(
				sff.FontName, sff.Size, fontSlant, fontWeight, sff.Spacing, sff.DefaultCharacter, fontChars, pngFile);

			if (!Directory.Exists(xnbFileName.VolumeAndDirectory))
			{
				Directory.CreateDirectory(xnbFileName.VolumeAndDirectory);
			}

			XnbFileWriterV5.WriteFile(sfc, xnbFileName);
		}

		static void SetupContext(Context g, string fontName, FontSlant fontSlant, FontWeight fontWeight, double fontSize)
		{
			FontOptions fo = new FontOptions();
			
			fo.Antialias = Antialias.Gray;
			fo.HintStyle = HintStyle.Full;
			
			g.FontOptions = fo;
			g.SelectFontFace(fontName, fontSlant, fontWeight);
			g.SetFontSize(fontSize);
			g.Color = new Color(1.0, 1.0, 1.0);
		}

		static List<CharacterData> CreateCharacterData(
			string fontName, 
			FontSlant fontSlant, 
			FontWeight fontWeight, 
			double fontSize, 
			List<char> fontChars, 
			out double bitmapWidth, 
			out double bitmapHeight)
		{
			List<CharacterData> cds = new List<CharacterData>();
			
			using (ImageSurface surface = new ImageSurface(Format.Argb32, 256, 256))
			{
				using (Context g = new Context(surface))
				{
					SetupContext(g, fontName, fontSlant, fontWeight, fontSize);
					
					FontExtents fe = g.FontExtents;
					double x = 0;
					double y = 0;
					
					for (int i = 0; i < fontChars.Count; i++)
					{
						CharacterData cd = new CharacterData();
						
						cds.Add(cd);
						cd.Character = new String(fontChars[i], 1);
						
						TextExtents te = g.TextExtents(cd.Character);
						double aliasSpace;

						if (cd.Character == " ")
							aliasSpace = 0.0;
						else
							aliasSpace = 1.0;

						cd.Bearing = new PointD(-te.XBearing + aliasSpace, -te.YBearing + aliasSpace);
						cd.Location = new Cairo.Rectangle(x, 0, te.Width + aliasSpace * 2, te.Height + aliasSpace * 2);
						cd.Cropping = new Cairo.Rectangle(0, fe.Ascent + aliasSpace * 2 - cd.Bearing.Y, te.XAdvance + aliasSpace, cd.Location.Height);
						cd.Kerning = new Vector3(0, (float)cd.Location.Width, (float)(cd.Bearing.X + cd.Cropping.Width - cd.Location.Width));
						
						x += cd.Location.Width;
						
						if (cd.Location.Height > y)
							y = cd.Location.Height;
					}
					
					bitmapWidth = x;
					bitmapHeight = y;
				}
			}
			
			return cds;
		}

		static BitmapContent CreateBitmapContent(
			double bitmapWidth, 
			double bitmapHeight, 
			string fontName, 
			FontSlant fontSlant, 
			FontWeight fontWeight, 
			double fontSize, 
			List<CharacterData> cds, 
			ParsedPath pngFile)
		{
			using (ImageSurface surface = new ImageSurface(Format.Argb32, (int)bitmapWidth, (int)bitmapHeight))
			{
				using (Context g = new Context(surface))
				{
					SetupContext(g, fontName, fontSlant, fontWeight, fontSize);
					double x = 0;

					for (int i = 0; i < cds.Count; i++)
					{
						CharacterData cd = cds[i];

						if (cd.Location.Width == 0)
							continue;

						g.MoveTo(x + cd.Bearing.X, cd.Bearing.Y);
						g.ShowText(cd.Character);
#if DEBUG
						g.Save();
						g.Color = new Color(1.0, 0, 0, 0.5);
						g.Antialias = Antialias.None;
						g.LineWidth = 1;
						g.MoveTo(x + 0.5, 0.5);
						g.LineTo(x + cd.Location.Width - 0.5, 0);
						g.LineTo(x + cd.Location.Width - 0.5, cd.Location.Height - 0.5);
						g.LineTo(x + 0.5, cd.Location.Height - 0.5);
						g.LineTo(x + 0.5, 0.5);
						g.Stroke();
						g.Restore();
#endif
						x += cd.Location.Width;
					}

					g.Restore();
				}

				if (pngFile != null)
					surface.WriteToPng(pngFile);

				return new BitmapContent(SurfaceFormat.Color, surface.Width, surface.Height, surface.Data);
			}
		}

		private SpriteFontContent CreateSpriteFontContent(
			string fontName, 
			double fontSize, 
			FontSlant fontSlant, 
			FontWeight fontWeight, 
			int spacing, 
			char? defaultChar, 
			List<char> fontChars,
			ParsedPath pngFile)
		{
			double bitmapWidth = 0;
			double bitmapHeight = 0;
			List<CharacterData> cds = CreateCharacterData(
				fontName, fontSlant, fontWeight, fontSize, fontChars, out bitmapWidth, out bitmapHeight);

			BitmapContent bitmapContent = CreateBitmapContent(
				bitmapWidth, bitmapHeight, fontName, fontSlant, fontWeight, fontSize, cds, pngFile);

			List<Microsoft.Xna.Framework.Rectangle> locations = new List<Microsoft.Xna.Framework.Rectangle>();
			List<Microsoft.Xna.Framework.Rectangle> croppings = new List<Microsoft.Xna.Framework.Rectangle>();
			List<Vector3> kernings = new List<Vector3>();

			foreach (var cd in cds)
			{
				locations.Add(new Microsoft.Xna.Framework.Rectangle(
					(int)cd.Location.X, (int)cd.Location.Y, (int)cd.Location.Width, (int)cd.Location.Height));
				croppings.Add(new Microsoft.Xna.Framework.Rectangle(
					(int)cd.Cropping.X, (int)cd.Cropping.Y, (int)cd.Location.Width, (int)cd.Location.Height));
				kernings.Add(new Vector3(
					(float)Math.Round(cd.Kerning.X, MidpointRounding.AwayFromZero),
					(float)Math.Round(cd.Kerning.Y, MidpointRounding.AwayFromZero),
					(float)Math.Round(cd.Kerning.Z, MidpointRounding.AwayFromZero)));
			}

			int verticalSpacing = 0;
			float horizontalSpacing = spacing;

			return new SpriteFontContent(
				new Texture2DContent(bitmapContent), 
				locations, 
				fontChars, 
				croppings, 
				verticalSpacing, 
				horizontalSpacing, 
				kernings, 
				defaultChar);
		}

        #endregion
    }
}
