using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
	public class SpriteFontContent
	{
		public SpriteFontContent(
			Texture2DContent texture, 
			List<Rectangle> glyphs,
			List<char> characterMap,
			List<Rectangle> cropping,
			int verticalSpacing,
			float horizontalSpacing,
			List<Vector3> kerning,
			char? defaultCharacter)
		{
			this.Texture = texture;
			this.Glyphs = glyphs;
			this.CharacterMap = characterMap;
			this.Cropping = cropping;
			this.VerticalSpacing = verticalSpacing;
			this.HorizontalSpacing = horizontalSpacing;
			this.Kerning = kerning;
			this.DefaultCharacter = defaultCharacter;
		}

		public Texture2DContent Texture { get; private set; }
		public List<Rectangle> Glyphs { get; private set; }
		public List<char> CharacterMap { get; private set; }
		public List<Rectangle> Cropping { get; private set; }
		public int VerticalSpacing { get; private set; }
		public float HorizontalSpacing { get; private set; }
		public List<Vector3> Kerning { get; private set; }
		public char? DefaultCharacter { get; private set; }
	}
}


