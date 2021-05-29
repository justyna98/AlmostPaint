using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostPaint.Drawings
{
    class Sector : Shape
    {
        public Point origin, b, c;
        public int radius;
        public Sector(Point a, Point b, Point c, Color ccol)
        {
            origin = a;
            this.b = b;
            this.c = c;
            radius = (int)Drawing.DistanceToPoint(a, b);
            ShapeColor = ccol;
        }
        public override string ToString()
        {
            return "sector";
        }
        public override void Draw()
        {
            Sector s = new Sector(origin, b, c, ShapeColor);
            Points = Drawing.DrawSector(s);
        }
        public int determinant(Point a, Point b, Point c)
        {
            int det = a.X * b.Y - a.X * c.Y - a.Y * b.X + a.Y * c.X + b.X * c.Y - b.Y * c.X;
            return det;
        }
        public List<Point> SectorDrawing()
        {
            int det = determinant(origin, b, c);

            if (radius == 0)
                return new List<Point>() { origin };

            var points = new List<Point>();
            int dE = 3;
            int dSE = 5 - 2 * radius;
            int y = radius, x = 0;
            int d = 1 - y;

            while (y > x)
            {
                if (d < 0)
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;
                if (det > 0) //inside
                {
                    DrawIn(new Point(origin.X, radius + origin.Y), points);
                    DrawIn(new Point(origin.X, -radius + origin.Y), points);
                    DrawIn(new Point(radius + origin.X, origin.Y),points);
                    DrawIn(new Point(-radius + origin.X, origin.Y), points);


                    DrawIn(new Point(y + origin.X, x + origin.Y), points);
                    DrawIn(new Point(-y + origin.X, x + origin.Y), points);
                    DrawIn(new Point(y + origin.X, -x + origin.Y), points);
                    DrawIn(new Point(-y + origin.X, -x + origin.Y), points);
                    DrawIn(new Point(x + origin.X, y + origin.Y), points);
                    DrawIn(new Point(-x + origin.X, y + origin.Y), points);
                    DrawIn(new Point(x + origin.X, -y + origin.Y), points);
                    DrawIn(new Point(-x + origin.X, -y + origin.Y), points);
                }
                if (det < 0) //outside
                {
                    DrawOut(new Point(origin.X, radius + origin.Y), points);
                    DrawOut(new Point(origin.X, -radius + origin.Y), points);
                    DrawOut(new Point(radius + origin.X, origin.Y), points);
                    DrawOut(new Point(-radius + origin.X, origin.Y), points);

                    DrawOut(new Point(y + origin.X, x + origin.Y), points);
                    DrawOut(new Point(-y + origin.X, x + origin.Y), points);
                    DrawOut(new Point(y + origin.X, -x + origin.Y), points);
                    DrawOut(new Point(-y + origin.X, -x + origin.Y), points);
                    DrawOut(new Point(x + origin.X, y + origin.Y), points);
                    DrawOut(new Point(-x + origin.X, y + origin.Y), points);
                    DrawOut(new Point(x + origin.X, -y + origin.Y), points);
                    DrawOut(new Point(-x + origin.X, -y + origin.Y), points);
                }
            }
            double lengthOC = Drawing.DistanceToPoint(origin, c);
            double final_len = radius;
            double ratio = final_len / lengthOC;
            Point end = new Point();
            end.X = (int)(origin.X + (c.X - origin.X) * ratio);
            end.Y = (int)(origin.Y + (c.Y - origin.Y) * ratio);
            Drawing.DrawLine(new Line(origin, b, ShapeColor, 1, false));
            Drawing.DrawLine(new Line(origin, end, ShapeColor, 1, false));
            return points;
        }
        public void DrawIn(Point d, List<Point> points)
        {
            if (determinant(origin, b, d) > 0 && determinant(origin, c, d) < 0)
            {
                Drawing.DrawPixel(d.X, d.Y, ShapeColor);
                points.Add(new Point(d.X, d.Y));
            }
        }
        public void DrawOut(Point d, List<Point> points)
        {
            if (determinant(origin, b, d) > 0 || determinant(origin, c, d) < 0)
            {
                Drawing.DrawPixel(d.X, d.Y, ShapeColor);
                points.Add(new Point(d.X, d.Y));
            }
        }
    }
}
