using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
namespace AlmostPaint
{
    class Line : Shape
    {
        public Point startPoint;
        public Point endPoint;

        public override string ToString()
        {
            return "Line";
        }
        public Line(Point start, Point end, Color c, int thick, bool AA)
        {
            startPoint = start;
            endPoint = end;
            Thickness = thick;
            ShapeColor = c;
            Points = null;
            antialiased = AA;
        }

        public override void Draw()
        {
            Line line = new Line(startPoint, endPoint, ShapeColor, Thickness, antialiased);
            Points = Drawing.DrawLine(line);
        }
        public List<Point> MidpointLine()
        {
            List<Point> points = new List<Point>();
            int x1 = startPoint.X, y1 = startPoint.Y;
            int x2 = endPoint.X, y2 = endPoint.Y;

            int x = x1, y = y1;
            int d, dx, dy, dE, dNE, xi, yi;

            dx = x2 - x1;
            dy = y2 - y1;
            xi = Math.Sign(dx); // 1 if right, -1 if left
            yi = Math.Sign(dy); // 1 if up, -1 if down
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            if (dx > dy) // more horizontal
            {
                d = dy * 2 - dx;
                dE = dy * 2;
                dNE = (dy - dx) * 2;
                while (x != x2)
                {
                    if (d < 0) // E - move column 
                    {
                        d += dE;
                        x += xi;
                    }
                    else // NE - move column and row
                    {
                        d += dNE;
                        x += xi;
                        y += yi;
                    }
                    Drawing.DrawPixel(x, y, ShapeColor);
                    points.Add(new Point(x, y));
                    // increasing thickness
                    for (int j = 0; j < Thickness / 2; j++)
                    {
                        Drawing.DrawPixel(x, y - j, ShapeColor);
                        Drawing.DrawPixel(x , y + j, ShapeColor);
                        points.Add(new Point(x , y - j));
                        points.Add(new Point(x , y + j));
                    }
                }
            }
            else // more vertical
            {
                d = dx * 2 - dy;
                dE = dx * 2;
                dNE = (dx - dy) * 2;
                while (y != y2)
                {
                    if (d < 0)
                    {
                        d += dE;
                        y += yi;
                    }
                    else
                    {
                        d += dNE;
                        x += xi;
                        y += yi;
                    }
                    Drawing.DrawPixel(x, y, ShapeColor);
                    points.Add(new Point(x, y));
                    // increasing thickness
                    for (int j = 0; j < Thickness / 2; j++)
                    {
                        Drawing.DrawPixel(x - j, y, ShapeColor);
                        Drawing.DrawPixel(x + j, y, ShapeColor);
                        points.Add(new Point(x - j, y));
                        points.Add(new Point(x + j, y));
                    }

                }
            }

            return points;
        }
        public List<Point> XiaolinWuLine()
        {
            List<Point> points = new List<Point>();
            var L = System.Windows.Media.Color.FromArgb((byte)ShapeColor.A, (byte)ShapeColor.R, (byte)ShapeColor.G, (byte)ShapeColor.B);
            var B = System.Windows.Media.Color.FromArgb((byte)255, (byte)255, (byte)255, (byte)255);
            int x1 = startPoint.X, y1 = startPoint.Y;
            int x2 = endPoint.X, y2 = endPoint.Y;
            float xinc, yinc, x, y;


            int dx = x2 - x1;
            int dy = y2 - y1;

            int steps = Math.Abs(dx) > Math.Abs(dy) ? Math.Abs(dx) : Math.Abs(dy);

            xinc = dx / (float)steps;
            yinc = dy / (float)steps;

            x = x1;
            y = y1;
            for (int k = 0; k <= steps; k++)
            {
                var c1 = L * (1 - (x - (int)Math.Truncate(x))) + B * (x - (int)Math.Truncate(x));
                var c2 = L * (x - (int)Math.Truncate(x)) + B * (1 - (x - (int)Math.Truncate(x)));
                var cc1 = System.Drawing.Color.FromArgb(c1.A, c1.R, c1.G, c1.B);
                var cc2 = System.Drawing.Color.FromArgb(c2.A, c2.R, c2.G, c2.B);
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    Drawing.DrawPixel((int)Math.Floor(x), (int)y, cc1);
                    points.Add(new Point((int)Math.Floor(x), (int)(y)));
                    Drawing.DrawPixel((int)Math.Floor(x), (int)(y) + 1, cc2);
                    points.Add(new Point((int)Math.Floor(x), (int)(y) + 1));
                }
                else
                {
                    Drawing.DrawPixel((int)Math.Floor(x), (int)(y), cc1);
                    points.Add(new Point((int)Math.Floor(x), (int)(y)));
                    Drawing.DrawPixel((int)Math.Floor(x)+1, (int)(y), cc2);
                    points.Add(new Point((int)Math.Floor(x)+1, (int)y));
                }
                x += xinc;
                y += yinc;

            }

            return points;
        }

    }
}
