using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;

namespace XnaBuildContent
{
    public static class PinboardFileWriter
    {
        public static void WriteXml(XmlWriter writer, PinboardFileV1 data)
        {
            WritePinboardXml(writer, data);
        }
               
        private static void WritePinboardXml(XmlWriter writer, PinboardFileV1 data)
        {
            writer.WriteStartElement("Pinboard");
            writer.WriteAttributeString("Format", "1");
            
            WriteRectangleXml(writer, data.ScreenRectInfo);

            WriteRectanglesXml(writer, data);
            writer.WriteEndElement();
        }

        private static void WriteRectanglesXml(XmlWriter writer, PinboardFileV1 data)
        {
            writer.WriteStartElement("Rectangles");

            foreach (var rectInfo in data.RectInfos)
            {
                WriteRectangleXml(writer, rectInfo);
            }

            writer.WriteEndElement();
        }

        private static void WriteRectangleXml(XmlWriter writer, PinboardFileV1.RectangleInfo rectInfo)
        {
            writer.WriteStartElement("Rectangle");
            writer.WriteElementString("Name", rectInfo.Name.ToString());
            writer.WriteElementString("X", rectInfo.X.ToString());
            writer.WriteElementString("Y", rectInfo.Y.ToString());
            writer.WriteElementString("Width", rectInfo.Width.ToString());
            writer.WriteElementString("Height", rectInfo.Height.ToString());
            WriteColorXml(writer, rectInfo.Color);
            writer.WriteEndElement();
        }

        private static void WriteColorXml(XmlWriter writer, Color color)
        {
            writer.WriteStartElement("Color");
            writer.WriteElementString("A", color.A.ToString());
            writer.WriteElementString("R", color.R.ToString());
            writer.WriteElementString("G", color.G.ToString());
            writer.WriteElementString("B", color.B.ToString());
            writer.WriteEndElement();
        }
    }
}

