using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using System.IO;

namespace XnaBuildContent
{
    public class StringsFileReaderV1
    {
        private XmlReader reader;
        private string stringsAtom;

        public StringsFileReaderV1(XmlReader reader)
        {
            this.reader = reader;
        }

        public static StringsFileV1 ReadFile(ParsedPath fileName)
        {
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                try
                {
                    return new StringsFileReaderV1(reader).ReadStringsElement();
                }
                catch (Exception e)
                {
                    e.Data["ContentFile"] = fileName;
                    e.Data["LineNumber"] = ((IXmlLineInfo)reader).LineNumber;
                    throw;
                }
            }
        }

        private StringsFileV1 ReadStringsElement()
        {
            StringsFileV1 stringsFile = new StringsFileV1();

            this.stringsAtom = this.reader.NameTable.Add("Strings");

            reader.MoveToContent();

            reader.ReadStartElement(stringsAtom);
            reader.MoveToContent();

            stringsFile.Strings = new List<StringsFileV1.String>();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, stringsAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                stringsFile.Strings.Add(ReadStringElement());
            }

            return stringsFile; 
        }

        private StringsFileV1.String ReadStringElement()
        {
            StringsFileV1.String s = new StringsFileV1.String();

            if (reader.IsStartElement("String"))
            {
                s.Name = reader.GetAttribute("Name");

                s.Value = reader.ReadElementContentAsString();
                reader.MoveToContent();
            }
            else
                throw new XmlException("Expected Start element");

            return s;
        }
    }
}

