using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pinboard
{
    public class PinboardData
    {
        public PinboardData()
        {
            RectInfos = new List<RectangleInfo>();
        }

        public RectangleInfo ScreenRectInfo { get; set; }
        public List<RectangleInfo> RectInfos { get; set; }

        public static PinboardData Default
        {
            get
            {
                PinboardData data = new PinboardData();
                
                data.ScreenRectInfo = new RectangleInfo(
                    new Rectangle(0, 0, 800, 600), "Screen", Color.White);

                return data;
            }
        }
    }

    public class RectangleInfo
    {
        public RectangleInfo()
        {
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
}
