using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using System.IO;

namespace XnaBuildContent
{
    public class ContentFileReaderV2
    {
		private string itemGroupAtom;
		private string propertyGroupAtom;
		private string targetAtom;
		private string contentAtom;
        private XmlReader reader;

        private ContentFileReaderV2(XmlReader reader)
        {
            itemGroupAtom = reader.NameTable.Add("ItemGroup");
            propertyGroupAtom = reader.NameTable.Add("PropertyGroup");
            targetAtom = reader.NameTable.Add("Target");
            contentAtom = reader.NameTable.Add("Content");

            this.reader = reader;
            this.reader.MoveToContent();
        }

        public static ContentFileV2 ReadFile(ParsedPath contentFile)
        {
            using (XmlReader reader = XmlReader.Create(contentFile))
            {
                try
                {
                    return new ContentFileReaderV2(reader).ReadContentElement();
                }
                catch (Exception e)
                {
                    e.Data["LineNumber"] = ((IXmlLineInfo)reader).LineNumber;
                    throw;
                }
            }
        }

        private ContentFileV2 ReadContentElement()
		{
			ContentFileV2 data = new ContentFileV2();

			string version = reader.GetAttribute("Version");

			if (String.IsNullOrEmpty(version))
				throw new XmlException("Version attribute not present");
			
			if (version != "2")
				throw new XmlException("Version attribute must be 2");
			
			reader.ReadStartElement(contentAtom);
			reader.MoveToContent();

			data.Items = new List<ContentFileV2.Item>();
			data.Targets = new List<ContentFileV2.Target>();

			while (true)
			{
				if (String.ReferenceEquals(reader.Name, contentAtom))
				{
		            reader.ReadEndElement();
		            reader.MoveToContent();
					break;
				}

				if (reader.NodeType == XmlNodeType.Element)
				{
					if (String.ReferenceEquals(reader.Name, itemGroupAtom))
					{
						List<ContentFileV2.Item> items = ReadItemGroupElement();

						data.Items.AddRange(items);
						continue;
					}
					else if (String.ReferenceEquals(reader.Name, targetAtom))
					{
						ContentFileV2.Target target = ReadTargetElement();

						foreach (var otherTarget in data.Targets)
						{
							if (String.CompareOrdinal(target.Name, otherTarget.Name) == 0)
								throw new XmlException("Duplicate target name '{0}'".CultureFormat(target.Name));
						}

						data.Targets.Add(target);
						continue;
					}
				}

				throw new XmlException("Expected ItemGroup, PropertyGroup or Target element");
			}

            return data;
        }

        private List<ContentFileV2.Item> ReadItemGroupElement()
        {
			List<ContentFileV2.Item> itemGroup = new List<ContentFileV2.Item>();

            // Read outer collection element
            reader.ReadStartElement(itemGroupAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, itemGroupAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

				ContentFileV2.Item item = ReadItemElement();

				itemGroup.Add(item);

				// Deal with an item that has and end element
				if (reader.NodeType == XmlNodeType.EndElement && reader.Name == item.Name)
				{
					reader.ReadEndElement();
					reader.MoveToContent();
				}
            }

            return itemGroup;
        }

        private ContentFileV2.Item ReadItemElement()
        {
			ContentFileV2.Item item = new ContentFileV2.Item();
			
			item.Name = reader.Name;
            item.Include = reader.GetAttribute("Include");

			if (String.IsNullOrEmpty(item.Include))
				throw new XmlException("Include attribute must be specified");

            item.Exclude = reader.GetAttribute("Exclude");

			reader.Skip();
            reader.MoveToContent();

			return item;
        }

		private ContentFileV2.Target ReadTargetElement()
		{
			ContentFileV2.Target target = new ContentFileV2.Target();

			target.LineNumber = ((IXmlLineInfo)reader).LineNumber;
			target.Name = reader.GetAttribute("Name");

			if (String.IsNullOrWhiteSpace(target.Name))
				throw new XmlException("Target 'Name' attribute must be set");

			target.Inputs = reader.GetAttribute("Inputs");

			if (target.Inputs == null)
				target.Inputs = String.Empty;

			target.Inputs = target.Inputs.Trim();

			target.Outputs = reader.GetAttribute("Outputs");

			if (String.IsNullOrWhiteSpace(target.Outputs))
				throw new XmlException("'Outputs' attribute must be set");

			target.Outputs = target.Outputs.Trim();
            
			reader.ReadStartElement();
			reader.MoveToContent();

			// Is there a nested PropertyGroup?
			if (reader.NodeType == XmlNodeType.Element && String.ReferenceEquals(propertyGroupAtom, reader.Name))
			{
				target.Properties = ReadPropertyGroupElement();
			}

			// Is there a separate Target end tag?
			if (reader.NodeType == XmlNodeType.EndElement && String.ReferenceEquals(targetAtom, reader.Name))
			{
				reader.ReadEndElement();
				reader.MoveToContent();
			}

            return target;
        }

        private List<Tuple<string, string>> ReadPropertyGroupElement()
        {
            List<Tuple<string, string>> properties = new List<Tuple<string, string>>();

            reader.ReadStartElement(propertyGroupAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, propertyGroupAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string key;
                string value;
                
                ReadPropertyElement(out key, out value);

                properties.Add(new Tuple<string, string>(key, value));
            }

            return properties;
        }

        private void ReadPropertyElement(out string key, out string value)
        {
            key = reader.Name;
            reader.MoveToContent();

            value = reader.ReadElementContentAsString();
            reader.MoveToContent();
        }
    }
}
