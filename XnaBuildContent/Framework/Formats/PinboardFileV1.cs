using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ToolBelt;

namespace XnaBuildContent
{
    public class PinboardFileV1
    {
        public class RectangleInfo
        {
            public RectangleInfo()
            {
            }

            public RectangleInfo(RectangleInfo other)
            {
                this.Name = other.Name;
                this.X = other.X;
                this.Y = other.Y;
                this.Width = other.Width;
                this.Height = other.Height;
                this.Color = other.Color;
            }

            public RectangleInfo(Rectangle rect, string name)
                : this(rect, name, Color.Gray)
            {
            }

            public RectangleInfo(Rectangle rect, string name, Color color)
            {
                this.X = rect.X;
                this.Y = rect.Y;
                this.Width = rect.Width;
                this.Height = rect.Height;
                this.Color = color;
                this.Name = name;
            }

            public string Name { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public Rectangle Rectangle
            {
                get
                {
                    return new Rectangle(X, Y, Width, Height);
                }
            }
            public Size Size
            {
                get
                {
                    return new Size(Width, Height);
                }
            }
            public Color Color { get; set; }
        }

        public PinboardFileV1()
        {
            RectInfos = new List<RectangleInfo>();
        }

        public RectangleInfo ScreenRectInfo { get; set; }
        public List<RectangleInfo> RectInfos { get; set; }

        public static PinboardFileV1 Default
        {
            get
            {
                PinboardFileV1 data = new PinboardFileV1();
                
                data.ScreenRectInfo = new RectangleInfo(
                    new Rectangle(0, 0, 800, 600), "Screen", Color.White);

                return data;
            }
        }

        public RectangleInfo GetRectangleInfoByName(string name)
        {
            if (name == "Screen")
                return this.ScreenRectInfo;
            else
                return this.RectInfos.Find(r => r.Name == name);
        }
    }
}

