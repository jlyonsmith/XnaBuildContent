using System;
using System.Xml;
using System.Collections.Generic;
using ToolBelt;

namespace XnaBuildContent
{
	public class SpriteFontFileReader
	{
		private XmlReader reader;
		private string xnaContentAtom;
		private string assetAtom;
		private string characterRegionsAtom;

		public SpriteFontFileReader(XmlReader reader)
		{
			this.reader = reader;
		}

		public static SpriteFontFile ReadFile(string fileName)
		{
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                try
                {
                    return new SpriteFontFileReader(reader).ReadXnaContentElement();
                }
                catch (Exception e)
                {
                    e.Data["ContentFile"] = fileName;
                    e.Data["LineNumber"] = ((IXmlLineInfo)reader).LineNumber;
                    throw;
                }
            }
		}

		private SpriteFontFile ReadXnaContentElement()
		{
			SpriteFontFile sff = new SpriteFontFile();

			this.xnaContentAtom = this.reader.NameTable.Add("XnaContent");
			this.assetAtom = this.reader.NameTable.Add("Asset");
			this.characterRegionsAtom = this.reader.NameTable.Add("CharacterRegions");

			reader.MoveToContent();

			reader.ReadStartElement(xnaContentAtom);
			reader.MoveToContent();

			reader.ReadStartElement(assetAtom);
			reader.MoveToContent();

			while (true)
			{
				if (reader.NodeType == XmlNodeType.EndElement &&
					String.ReferenceEquals(reader.Name, this.assetAtom))
				{
					reader.ReadEndElement();
					reader.MoveToContent();
					break;
				}

				if (reader.NodeType != XmlNodeType.Element)
					throw new XmlException("Expected an element");

				switch (reader.Name)
				{
				case "FontName":
					sff.FontName = reader.ReadElementContentAsString();
					break;
				case "Size":
					sff.Size = reader.ReadElementContentAsInt();
					break;
				case "Spacing":
					sff.Spacing = reader.ReadElementContentAsInt();
					break;
				case "UseKerning":
					sff.UseKerning = reader.ReadElementContentAsBoolean();
					break;
				case "Style":
				{
					SpriteFontFile.FontStyle result;
					string value = reader.ReadElementContentAsString();

					if (SpriteFontFile.FontStyle.TryParse(value, out result))
						sff.Style = result;
					else
						throw new XmlException("Unrecognized font Style value".CultureFormat(value));

					break;
				}
				case "DefaultCharacter":
				{
					string value = reader.ReadElementContentAsString();

					if (value.Length > 0)
						sff.DefaultCharacter = value[0];
					else
						throw new XmlException("DefaultCharacter element is empty");

					break;
				}
				case "CharacterRegions":
					sff.CharacterRegions = ReadCharacterRegionsElement();
					continue;
				default:
					throw new XmlException("Unexpected element '{0}'".CultureFormat(reader.Name));
				}

				reader.MoveToContent();
			}

			reader.ReadEndElement(); // XnaContent
			reader.MoveToContent();

			return sff;
		}

		private List<SpriteFontFile.CharacterRegion> ReadCharacterRegionsElement()
		{
			List<SpriteFontFile.CharacterRegion> regions = new List<SpriteFontFile.CharacterRegion>();

			reader.ReadStartElement();
			reader.MoveToContent();

			while (true)
			{
				if (reader.NodeType == XmlNodeType.EndElement && 
				    String.ReferenceEquals(reader.Name, characterRegionsAtom))
				{
					reader.ReadEndElement();
					reader.MoveToContent();
					break;
				}

				char start;
				char end;

				ReadCharacterRegionElement(out start, out end);

				regions.Add(new SpriteFontFile.CharacterRegion(start, end));
			}

			return regions;
		}

		private void ReadCharacterRegionElement(out char start, out char end)
		{
			reader.ReadStartElement("CharacterRegion");
			reader.MoveToContent();

			string s;

			s = reader.ReadElementContentAsString("Start", "");
			reader.MoveToContent();

			if (s.Length > 0)
				start = s[0];
			else
				throw new XmlException("Missing Start character");

			s = reader.ReadElementContentAsString("End", "");
			reader.MoveToContent();

			if (s.Length > 0)
				end = s[0];
			else
				throw new XmlException("Missing End character");

			reader.ReadEndElement();
			reader.MoveToContent();
		}
	}
}


