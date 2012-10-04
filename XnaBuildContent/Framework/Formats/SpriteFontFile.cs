using System;
using System.Collections.Generic;

namespace XnaBuildContent
{
	public class SpriteFontFile
	{
		public enum FontStyle
		{
			Regular,
			Italic,
			Bold
		}

		public class CharacterRegion
		{
			public CharacterRegion(char start, char end)
			{
				if (start > end)
					// Start must come before end
					throw new ArgumentException();

				Start = start;
				End = end;
			}

			public char Start { get; private set; }
			public char End { get; private set; }
		}

		public string FontName { get; set; }
		public int Size { get; set; }
		public int Spacing { get; set; }
		public bool UseKerning { get; set; }
		public FontStyle Style { get; set; }
		public char? DefaultCharacter { get; set; }
		public IList<CharacterRegion> CharacterRegions { get; set; }
	}
}


