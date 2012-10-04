using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;

namespace Pinboard
{
    public static class PinboardDataReaderV1
    {
        public static string rectanglesAtom;

        public static PinboardData ReadXml(XmlReader reader)
        {
            rectanglesAtom = reader.NameTable.Add("Rectangles");
            
            reader.MoveToContent();
            PinboardData data = ReadPinboardXml(reader);
            return data;
        }

        private static PinboardData ReadPinboardXml(XmlReader reader)
        {
            PinboardData data = new PinboardData();
            
            reader.ReadStartElement("Pinboard");
            reader.MoveToContent();
            data.ScreenRectInfo = ReadRectangleXml(reader);

            data.RectInfos = ReadRectanglesXml(reader);
            
            reader.ReadEndElement();
            reader.MoveToContent();

            return data;
        }

        private static List<RectangleInfo> ReadRectanglesXml(XmlReader reader)
        {
            List<RectangleInfo> list = new List<RectangleInfo>();

            // Read <Rectangles>
            reader.ReadStartElement(rectanglesAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, rectanglesAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                RectangleInfo rectInfo = ReadRectangleXml(reader);
                
                list.Add(rectInfo);
            }

            return list;
        }

        private static RectangleInfo ReadRectangleXml(XmlReader reader)
        {
            RectangleInfo rectInfo = new RectangleInfo();

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
            rectInfo.Color = ReadColorXml(reader);
            reader.ReadEndElement();
            reader.MoveToContent();

            return rectInfo;
        }

        private static Color ReadColorXml(XmlReader reader)
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
