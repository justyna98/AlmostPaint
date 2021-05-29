using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostPaint
{
    class Filling
    {
        public class ActiveEdge
        {
            public int yMax;
            public double x;
            public double inverseM;
            public ActiveEdge(Point b, Point a)
            {
                yMax = b.Y;
                x = (double)a.X;
                inverseM= ((double)(b.X - a.X)) / ((double)(b.Y - a.Y));
            }
        }
        private static Point LowerY(Point p1, Point p2)
        {
            if (p1.Y <= p2.Y) return p1; else return p2;
        }
        private static Point UpperY(Point p1, Point p2)
        {
            if (p1.Y >= p2.Y) return p1; else return p2;
        }
        //Scanline algorithm with ActiveEdgeTable - fill with color
        public static void FillPolygon(List<Point> vertices, Color color)
        {
            int verticesCount = vertices.Count();
            //initialize AET to empty
            List<ActiveEdge> AET = new List<ActiveEdge>();
            List<int> ordered = new List<int>();
            for (int j = 0;j < verticesCount; j++)
            {
                ordered.Add(j);
            }
            //sort by y
            ordered = ordered.OrderBy(p => vertices[p].Y).ToList();
            int k = 0;
            int i = ordered[k];
            int y = vertices[ordered[0]].Y;
            int ymax = vertices[ordered[verticesCount-1]].Y;
            Drawing.WriteableBitmap.Lock();
            while (y < ymax)
            {
                while (vertices[i].Y == y)
                {
                    if (i > 0)
                    {
                        if (vertices[i - 1].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[i - 1], vertices[i]);
                            var u = UpperY(vertices[i - 1], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    else
                    {
                        if (vertices[verticesCount - 1].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[verticesCount - 1], vertices[i]);
                            var u = UpperY(vertices[verticesCount - 1], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    if (i < verticesCount - 1)
                    {
                        if (vertices[i + 1].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[i + 1], vertices[i]);
                            var u = UpperY(vertices[i + 1], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    else
                    {
                        if (vertices[0].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[0], vertices[i]);
                            var u = UpperY(vertices[0], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    i = ordered[++k];
                }
                //sort AET by x value
                AET = AET.OrderBy(p => p.x).ToList();
                //fill pixels between pairs of intersections
                for (int j = 0; j < AET.Count; j += 2)
                {
                    if (j + 1 < AET.Count)
                    {
                        for (int x = (int)AET[j].x; x <= (int)AET[j + 1].x; x++)
                        {
                            Drawing.DrawPixel(x, y, color);
                        }
                    }
                }
                ++y;
                //remove from AET edges for which ymax = y
                AET.RemoveAll(x => (x.yMax == y));
                //x += 1 / m
                for (int j = 0; j < AET.Count; j++)
                    AET[j].x += AET[j].inverseM;
            }
            Drawing.WriteableBitmap.Unlock();
        }
        //Scanline algorithm with ActiveEdgeTable - fill with image
        public static unsafe void FillPolygon(List<Point> vertices, Bitmap pattern)
        {
            int verticesCount = Enumerable.Count<Point>(vertices);
            List<ActiveEdge> AET = new List<ActiveEdge>();
            var P1 = Enumerable.OrderBy<Point, int>(vertices, (Func<Point, int>)(p => (int)p.Y)).ToList();
            List<int> ordered = new List<int>();
            for (int j = 0; j < verticesCount; j++)
            {
                ordered.Add(j);
            }
            ordered = ordered.OrderBy(p => vertices[p].Y).ToList();
            int k = 0;
            int i = ordered[k];
            int y, ymin, ymax, xmin;
            y = ymin = vertices[ordered[0]].Y;
            ymax = vertices[ordered[verticesCount - 1]].Y;
            xmin = Enumerable.OrderBy<Point, int>(vertices, (Func<Point, int>)(p => (int)p.X)).First().X;

            BitmapData bData = pattern.LockBits(
                    new System.Drawing.Rectangle(0, 0, (int)pattern.Width, (int)pattern.Height), ImageLockMode.ReadOnly, pattern.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();
            byte bitsPerPixel = (byte)(Image.GetPixelFormatSize(bData.PixelFormat));

            Drawing.WriteableBitmap.Lock();
            while (y < ymax)
            {
                while (vertices[i].Y == y)
                {
                    // remember to wrap indices in polygon                                    
                    if (i > 0)
                    {
                        if (vertices[i - 1].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[i - 1], vertices[i]);
                            var u = UpperY(vertices[i - 1], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    else
                    {
                        if (vertices[verticesCount - 1].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[verticesCount - 1], vertices[i]);
                            var u = UpperY(vertices[verticesCount - 1], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    if (i < verticesCount - 1)
                    {
                        if (vertices[i + 1].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[i + 1], vertices[i]);
                            var u = UpperY(vertices[i + 1], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    else
                    {
                        if (vertices[0].Y > vertices[i].Y)
                        {
                            var l = LowerY(vertices[0], vertices[i]);
                            var u = UpperY(vertices[0], vertices[i]);
                            AET.Add(new ActiveEdge(u, l));
                        }
                    }
                    ++k;
                    i = ordered[k];
                }
                //sort AET by x value
                AET = AET.OrderBy(p => p.x).ToList();
                //fill pixels between pairs of intersections
                for (int j = 0; j < AET.Count; j += 2)
                {
                    if (j + 1 < AET.Count)
                    {
                        for (int x = (int)AET[j].x; x <= (int)AET[j + 1].x; x++)
                        {
                            Color color = new Color();
                            unsafe
                            {
                                byte* tmp = scan0 + ((y - ymin) % bData.Height) * bData.Stride + ((x - xmin) % bData.Width) * bitsPerPixel / 8;
                                color= System.Drawing.Color.FromArgb(255, tmp[2], tmp[1], tmp[0]);
                            }
                            Drawing.DrawPixel(x, y, color);
                        }
                    }
                }
                ++y;
                //remove from AET edges for which ymax = y
                AET.RemoveAll(x => (x.yMax == y));
                //for each edge in AET
                //x += 1 / m
                for (int j = 0; j < AET.Count; j++)
                   AET[j].x += AET[j].inverseM;
            }
            pattern.UnlockBits(bData);
            Drawing.WriteableBitmap.Unlock();
        }

        // 4-connected pixels Boundary Fill without recursion
        public static void boundaryFill(int x, int y,Color fillingColor, Color drawingColor)
        {
            if (x < 0 || x >= Drawing.WriteableBitmap.PixelWidth || y < 0 || y >= Drawing.WriteableBitmap.PixelHeight)
            {
                return;
            }

            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(x, y));
            Drawing.WriteableBitmap.Lock();
            while (q.Count != 0)
            {
                var element = q.Dequeue(); ;
                if (Drawing.GetColor(element.X, element.Y) != drawingColor && Drawing.GetColor(element.X, element.Y) != fillingColor)
                {
                    Drawing.DrawPixel(element.X, element.Y, fillingColor);
                    if (element.X + 1 < Drawing.WriteableBitmap.PixelWidth)
                    {
                        q.Enqueue(new Point(element.X + 1, element.Y));
                    }
                    if (element.X - 1 >= 0)
                    {
                        q.Enqueue(new Point(element.X - 1, element.Y));
                    }
                    if (element.Y + 1 < Drawing.WriteableBitmap.PixelHeight)
                    {
                        q.Enqueue(new Point(element.X, element.Y + 1));
                    }
                    if (element.Y - 1 >= 0)
                    {
                        q.Enqueue(new Point(element.X, element.Y - 1));
                    }

                }

            }

            Drawing.WriteableBitmap.Unlock();
        }
    }
}
