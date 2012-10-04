using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;
using ToolBelt;

namespace XnaBuildContent
{
    public class PinboardFileReaderV1
    {
        public string rectanglesAtom;
        private XmlReader reader;

        private PinboardFileReaderV1(XmlReader reader)
        {
            rectanglesAtom = reader.NameTable.Add("Rectangles");
            
            this.reader = reader;
            this.reader.MoveToContent();
        }

        public static PinboardFileV1 ReadFile(ParsedPath contentFile)
        {
            using (XmlReader reader = XmlReader.Create(contentFile))
            {
                return new PinboardFileReaderV1(reader).ReadPinboardXml();
            }
        }

        private PinboardFileV1 ReadPinboardXml()
        {
            PinboardFileV1 data = new PinboardFileV1();
            
            reader.ReadStartElement("Pinboard");
            reader.MoveToContent();
            data.ScreenRectInfo = ReadRectangleXml();

            data.RectInfos = ReadRectanglesXml();
            
            reader.ReadEndElement();
            reader.MoveToContent();

            return data;
        }

        private List<PinboardFileV1.RectangleInfo> ReadRectanglesXml()
        {
            List<PinboardFileV1.RectangleInfo> list = new List<PinboardFileV1.RectangleInfo>();
            bool empty = reader.IsEmptyElement;

            reader.ReadStartElement(rectanglesAtom);
            reader.MoveToContent();

            if (!empty)
            {
                while (true)
                {
                    if (String.ReferenceEquals(reader.Name, rectanglesAtom))
                    {
                        reader.ReadEndElement();
                        reader.MoveToContent();
                        break;
                    }

                    PinboardFileV1.RectangleInfo rectInfo = ReadRectangleXml();

                    list.Add(rectInfo);
                }
            }

            return list;
        }

        private PinboardFileV1.RectangleInfo ReadRectangleXml()
        {
            PinboardFileV1.RectangleInfo rectInfo = new PinboardFileV1.RectangleInfo();

            // Read <Rectangle>
            reader.ReadStartElement("Rectangle");
            reader.MoveToContent();
            rectInfo.Name = reader.ReadElementContentAsString("Name", "");
            reader.MoveToContent();
            rectInfo.X = reader.ReadElementContentAsInt("X", "");
            reader.MoveToContent();
            rectInfo.Y = reader.ReadElementContentAsInt("Y", "");
            reader.MoveToContent();
            rectInfo.Width = reader.ReadElementContentAsInt("Width", "");
            reader.MoveToContent();
            rectInfo.Height = reader.ReadElementContentAsInt("Height", "");
            reader.MoveToContent();
            rectInfo.Color = ReadColorXml();
            reader.ReadEndElement();
            reader.MoveToContent();

            return rectInfo;
        }

        private Color ReadColorXml()
        {
            int a, r, g, b;

            reader.ReadStartElement("Color");
            reader.MoveToContent();
            a = reader.ReadElementContentAsInt("A", "");
            reader.MoveToContent();
            r = reader.ReadElementContentAsInt("R", "");
            reader.MoveToContent();
            g = reader.ReadElementContentAsInt("G", "");
            reader.MoveToContent();
            b = reader.ReadElementContentAsInt("B", "");
            reader.MoveToContent();
            reader.ReadEndElement();
            reader.MoveToContent();

            return Color.FromArgb(a, r, g, b);
        }
    }
}

