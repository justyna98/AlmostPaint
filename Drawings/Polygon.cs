using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostPaint
{

    class Polygon : Shape
    {
        public List<Point> vertices { get; set; }
        public Color? FillColor = null;
        public Bitmap FillPattern = null;
        
        public Polygon(Color c, int thick, bool AA)
        {
            vertices = new List<Point>();
            ShapeColor = c;
            Points = null;
            Thickness = thick;
            antialiased = AA;
        }


        public override string ToString()
        {
            return "Polygon";
        }
        public void Add(Point point)
        {
            vertices.Add(point);
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
            for (int i = 0; i < vertices.Count() - 1; i++)
            {
                Line l1 = new Line(vertices[i], vertices[i + 1], ShapeColor, Thickness, antialiased);
                Points.AddRange(Drawing.DrawLine(l1));
            }
            Line l2 = new Line(vertices[vertices.Count - 1], vertices[0], ShapeColor, Thickness, antialiased);
            Points.AddRange(Drawing.DrawLine(l2));
        }
    }
}
