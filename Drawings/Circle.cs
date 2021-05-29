using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace AlmostPaint
{
    class Circle : Shape
    {
        public Point origin;
        public int radius;
        public Circle(Point o, int r, Color c, bool AA)
        {
            origin = o;
            radius = r;
            ShapeColor = c;
            antialiased = AA;
        }
        public override string ToString()
        {
            return "Circle";
        }
        public override void Draw()
        {
            Circle c = new Circle(origin, radius, ShapeColor, antialiased);
            Points = Drawing.DrawCircle(c);
        }

        public List<Point> MidpointCircleAddition()
        {
            if (radius == 0)
                return new List<Point>() { origin };

            var points = new List<Point>();
            int dE = 3;
            int dSE = 5 - 2 * radius;
            int y = radius, x = 0;
            int d = 1 - y;
            Drawing.DrawPixel(origin.X, radius + origin.Y, ShapeColor);
            Drawing.DrawPixel(origin.X, -radius + origin.Y, ShapeColor);
            Drawing.DrawPixel(radius + origin.X, origin.Y, ShapeColor);
            Drawing.DrawPixel(-radius + origin.X, origin.Y, ShapeColor);
            points.Add(new Point(origin.X, radius + origin.Y));
            points.Add(new Point(origin.X, -radius + origin.Y));
            points.Add(new Point(radius + origin.X, origin.Y));
            points.Add(new Point(-radius + origin.X, origin.Y));

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
                Drawing.DrawPixel(y + origin.X, x + origin.Y, ShapeColor);
                Drawing.DrawPixel(-y + origin.X, x + origin.Y, ShapeColor);
                Drawing.DrawPixel(y + origin.X, -x + origin.Y, ShapeColor);
                Drawing.DrawPixel(-y + origin.X, -x + origin.Y, ShapeColor);
                Drawing.DrawPixel(x + origin.X, y + origin.Y, ShapeColor);
                Drawing.DrawPixel(-x + origin.X, y + origin.Y, ShapeColor);
                Drawing.DrawPixel(x + origin.X, -y + origin.Y, ShapeColor);
                Drawing.DrawPixel(-x + origin.X, -y + origin.Y, ShapeColor);
                points.Add(new Point(y + origin.X, x + origin.Y));
                points.Add(new Point(-y + origin.X, x + origin.Y));
                points.Add(new Point(y + origin.X, -x + origin.Y));
                points.Add(new Point(-y + origin.X, -x + origin.Y));
                points.Add(new Point(x + origin.X, y + origin.Y));
                points.Add(new Point(-x + origin.X, y + origin.Y));
                points.Add(new Point(x + origin.X, -y + origin.Y));
                points.Add(new Point(-x + origin.X, -y + origin.Y));
            }

            return points;
        }
        public List<Point> XiaolinWuCircle()
        {
            if (radius == 0)
                return new List<Point>() { origin };

            var points = new List<Point>();
            var L = System.Windows.Media.Color.FromArgb((byte)ShapeColor.A, (byte)ShapeColor.R, (byte)ShapeColor.G, (byte)ShapeColor.B);
            var B = System.Windows.Media.Color.FromArgb((byte)255, (byte)255, (byte)255, (byte)255);
            int x = radius, y = 0;

            Drawing.DrawPixel(origin.X + radius, origin.Y, ShapeColor);
            Drawing.DrawPixel(origin.X - radius, origin.Y, ShapeColor);
            Drawing.DrawPixel(origin.X, origin.Y + radius, ShapeColor);
            Drawing.DrawPixel(origin.X, origin.Y - radius, ShapeColor);

            points.Add(new Point(origin.X + radius, origin.Y));
            points.Add(new Point(origin.X - radius, origin.Y));
            points.Add(new Point(origin.X, origin.Y + radius));
            points.Add(new Point(origin.X, origin.Y - radius));
            while (x > y)
            {
                ++y;
                x = (int)Math.Ceiling(Math.Sqrt(radius * radius - y * y));
                float T = (float)(x - Math.Sqrt(radius * radius - y * y));
                var c1 = L * (1 - T) + B * T;
                var c2 = L * T + B * (1 - T);
                var cc1 = System.Drawing.Color.FromArgb(c1.A, c1.R, c1.G, c1.B);
                var cc2 = System.Drawing.Color.FromArgb(c2.A, c2.R, c2.G, c2.B);

                Drawing.DrawPixel(origin.X + x, origin.Y + y, cc1);
                Drawing.DrawPixel(origin.X + x, origin.Y - y, cc1);
                Drawing.DrawPixel(origin.X - x, origin.Y + y, cc1);
                Drawing.DrawPixel(origin.X - x, origin.Y - y, cc1);

                Drawing.DrawPixel(origin.X + y, origin.Y + x, cc1);
                Drawing.DrawPixel(origin.X + y, origin.Y - x, cc1);
                Drawing.DrawPixel(origin.X - y, origin.Y + x, cc1);
                Drawing.DrawPixel(origin.X - y, origin.Y - x, cc1);

                Drawing.DrawPixel(origin.X + x - 1, origin.Y + y, cc2);
                Drawing.DrawPixel(origin.X + x - 1, origin.Y - y, cc2);
                Drawing.DrawPixel(origin.X - x + 1, origin.Y + y, cc2);
                Drawing.DrawPixel(origin.X - x + 1, origin.Y - y, cc2);

                Drawing.DrawPixel(origin.X + y, origin.Y + x - 1, cc2);
                Drawing.DrawPixel(origin.X + y, origin.Y - x + 1, cc2);
                Drawing.DrawPixel(origin.X - y, origin.Y + x - 1, cc2);
                Drawing.DrawPixel(origin.X - y, origin.Y - x + 1, cc2);

                //adding points
                points.Add(new Point(origin.X + x, origin.Y + y));
                points.Add(new Point(origin.X + x, origin.Y - y));
                points.Add(new Point(origin.X - x, origin.Y + y));
                points.Add(new Point(origin.X - x, origin.Y - y));

                points.Add(new Point(origin.X + y, origin.Y + x));
                points.Add(new Point(origin.X + y, origin.Y - x));
                points.Add(new Point(origin.X - y, origin.Y + x));
                points.Add(new Point(origin.X - y, origin.Y - x));

                points.Add(new Point(origin.X + x - 1, origin.Y + y));
                points.Add(new Point(origin.X + x - 1, origin.Y - y));
                points.Add(new Point(origin.X - x + 1, origin.Y + y));
                points.Add(new Point(origin.X - x + 1, origin.Y - y));

                points.Add(new Point(origin.X + y, origin.Y + x - 1));
                points.Add(new Point(origin.X + y, origin.Y - x + 1));
                points.Add(new Point(origin.X - y, origin.Y + x - 1));
                points.Add(new Point(origin.X - y, origin.Y - x + 1));
            }



            return points;
        }

    }
}
