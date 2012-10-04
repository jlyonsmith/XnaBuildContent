using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Jamoki.Tools.Pinboard
{
    public class Vector
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Vector(Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public Vector(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static implicit operator Vector(Point p)
        {
            return new Vector(p);
        }

        public static double Length(Vector v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        public static int Dot(Vector v0, Vector v1)
        {
            return v0.X * v1.X + v0.Y + v1.Y;
        }

        public static bool IsPointInTriangleOld(Point P, Point A, Point B, Point C)
        {
            // Compute vectors        
            Vector v0 = C - (Size)A;
            Vector v1 = B - (Size)A;
            Vector v2 = P - (Size)A;

            // Compute dot products
            int dot00 = Vector.Dot(v0, v0);
            int dot01 = Vector.Dot(v0, v1);
            int dot02 = Vector.Dot(v0, v2);
            int dot11 = Vector.Dot(v1, v1);
            int dot12 = Vector.Dot(v1, v2);

            // Compute barycentric coordinates
            float invDenom = 1 / (float)(dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        public static bool IsPointInTriangle(Point p, Point a, Point b, Point c)
        {
            // Compute barycentric coordinates
            float det = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            float u = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / det;
            float v = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / det;

            // Check if point is in triangle
            return (u > 0) && (v > 0) && (u + v < 1);
        }
    }
}
