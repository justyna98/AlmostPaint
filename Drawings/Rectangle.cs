using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostPaint
{
    class Rectangle : Shape
    {
        public Point Corner1, Corner2;
        public List<Point> vertices;
        public Color? FillColor = null;
        public Bitmap FillPattern = null;
        public Rectangle(Point a, Point b, Color c, int thick, bool AA)
        {
            vertices = new List<Point>();
            Corner1 = a;
            Corner2 = b;
            ShapeColor = c;
            Thickness = thick;
            antialiased = AA;
            vertices.Add(Corner1);
            vertices.Add(new Point(Corner2.X , Corner1.Y));
            vertices.Add(Corner2);
            vertices.Add(new Point(Corner1.X, Corner2.Y));

        }
        public override string ToString()
        {
            return "Rectangle";
        }
        public override void Draw()
        {
            if (FillColor != null)
            {
                Filling.FillPolygon(vertices, (Color)FillColor);
            }
            else if (FillPattern != null)
            {
                Filling.FillPolygon(vertices, FillPattern);
            }
            Points = new List<Point>();
            Line l1 = new Line(vertices[0],vertices[1] ,ShapeColor, Thickness, antialiased);
            Points.AddRange(Drawing.DrawLine(l1));
            Line l2 = new Line(vertices[1], vertices[2], ShapeColor, Thickness, antialiased);
            Points.AddRange(Drawing.DrawLine(l2));
            Line l3 = new Line(vertices[2], vertices[3], ShapeColor, Thickness, antialiased);
            Points.AddRange(Drawing.DrawLine(l3));
            Line l4 = new Line(vertices[0], vertices[3], ShapeColor, Thickness, antialiased);
            Points.AddRange(Drawing.DrawLine(l4));

        }
    }
}
