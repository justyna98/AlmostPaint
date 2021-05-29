using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Point = System.Drawing.Point;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AlmostPaint.Drawings;

namespace AlmostPaint
{
    class Drawing
    {
        public static WriteableBitmap WriteableBitmap { get; set; }
        public static Point? PolyFirstPoint=null;
        public static Rectangle ClippingShape=null;
        public static Shape ClippedShape = null;
        public static System.Drawing.Color color;
        public static bool clipping = false;
        internal static Color GetColor(int x, int y)
        {
            var color = new Color();
            if (x < 0 || y < 0 || x >= WriteableBitmap.PixelWidth || y >= WriteableBitmap.PixelHeight)
                return color;
            unsafe
            {
                IntPtr pBackBuffer = WriteableBitmap.BackBuffer + y * WriteableBitmap.BackBufferStride + x * 4;

                int color_data = *((int*)pBackBuffer);
                var B = (byte)((color_data & 0x000000FF) >> 0);
                var G = (byte)((color_data & 0x0000FF00) >> 8);
                var R = (byte)((color_data & 0x00FF0000) >> 16);
                var A = (byte)((color_data & 0xFF000000) >> 24);
                color= System.Drawing.Color.FromArgb(A,R,G,B);
            }
            return color;
        }
        public static void DrawPixel(int x, int y, Color c)
        {
            //check the bounds of bitmap
            if(x<0 || x>= WriteableBitmap.PixelWidth || y<0 || y >= WriteableBitmap.PixelHeight)
            {
                return;
            }
            unsafe
            {
                // Get a pointer to the back buffer.
                IntPtr pBackBuffer = WriteableBitmap.BackBuffer + y * WriteableBitmap.BackBufferStride + x * 4;

                // Compute the pixel's color.
                int color_data = 0;
                color_data |= c.A << 24;  // A
                color_data |= c.R << 16;  // R
                color_data |= c.G << 8;   // G
                color_data |= c.B << 0;   // B

                // Assign the color data to the pixel.
                *((int*)pBackBuffer) = color_data;
            }
            WriteableBitmap.AddDirtyRect(new Int32Rect(x,y, 1, 1));
        }

        public static List<Point> DrawLine(Line l)
        {
            List<Point> points;
            WriteableBitmap.Lock();
            if (l.antialiased==true)
            {
                points = l.XiaolinWuLine();
            }
            else
            {
                points = l.MidpointLine();
            }
            WriteableBitmap.Unlock();
            return points;
        }

        public static List<Point> DrawCircle(Circle c)
        {
            List<Point> points;
            WriteableBitmap.Lock();
            if (c.antialiased==true)
            {
                points = c.XiaolinWuCircle();
            }
            else
            {
                points = c.MidpointCircleAddition();
            }
            WriteableBitmap.Unlock();
            return points;
        }
        public static List<Point> DrawSector(Sector s)
        {
            List<Point> points;
            WriteableBitmap.Lock();
            points = s.SectorDrawing();
            WriteableBitmap.Unlock();
            return points;
        }
        internal static void CleanBitmap()
        {
            try
            {
                WriteableBitmap.Lock();
                var backBuffer = WriteableBitmap.BackBuffer;
                unsafe
                {
                    for (int y = 0; y < WriteableBitmap.PixelHeight; y++)
                    {
                        for (int x = 0; x < WriteableBitmap.PixelWidth; x++)
                        {
                            var bufPtr = backBuffer + WriteableBitmap.BackBufferStride * y + x * 4;

                            int color_data = 0;
                            color_data |= 255 << 24;    // A
                            color_data |= 255 << 16;    // R
                            color_data |= 255 << 8;     // G
                            color_data |= 255 << 0;     // B

                            *((int*)bufPtr) = color_data;
                        }
                    }
                }
                WriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, WriteableBitmap.PixelWidth, WriteableBitmap.PixelHeight));
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }
        public static void Redraw(List<Shape> shapes)
        {
            CleanBitmap();
            Clipping Clipping = new Clipping();
            foreach (var shape in shapes)
            {
                shape.Draw();
            }
            if( ClippedShape!=null && ClippingShape != null && clipping)
            {
                if (ClippedShape is Line)
                {
                    var l = ClippedShape as Line;
                    Clipping.LiangBarsky(Drawing.ClippingShape, l.startPoint, l.endPoint, color);
                }
                if (ClippedShape is Rectangle)
                {
                    var rect = ClippedShape as Rectangle;
                    var vertices = rect.vertices;
                    for (int i = 0; i < vertices.Count() - 1; i++)
                    {
                        Clipping.LiangBarsky(Drawing.ClippingShape, vertices[i], vertices[i + 1], color);
                    }
                    Clipping.LiangBarsky(Drawing.ClippingShape, vertices[0], vertices[vertices.Count - 1], color);
                }
                if (ClippedShape is Polygon)
                {
                    var poly = ClippedShape as Polygon;
                    var vertices = poly.vertices;
                    for (int i = 0; i < vertices.Count() - 1; i++)
                    {
                        Clipping.LiangBarsky(Drawing.ClippingShape, vertices[i], vertices[i + 1], color);
                    }
                    Clipping.LiangBarsky(Drawing.ClippingShape, vertices[0], vertices[vertices.Count - 1], color);
                }
            }
        }

        public static Shape GetShape(List<Shape> shapes, int x, int y)
        {
            Shape FoundShape = null;
            int distance = 10;

            foreach (var shape in shapes)
            {
                foreach(var point in shape.Points)
                {
                    for(int i=0; i < distance; i++)
                    {
                        for(int j=0;j< distance; j++)
                        {
                            int howfar = (int)Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
                            if (howfar < distance)
                            {
                                distance = howfar;
                                FoundShape = shape;
                                if (howfar == 0)
                                {
                                    return FoundShape;
                                }
                            }
                        }
                    }
                }
            }
            return FoundShape;
        }
        public static double DistanceToPoint(int x, int y, int x2, int y2)
        {
            if (y == y2)
            {
                return Math.Abs(x2 - x);
            }
            else return Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2));

        }
        public static double DistanceToPoint(Point p1, Point p2)
        {
            return DistanceToPoint(p1.X, p1.Y, p2.X, p2.Y);

        }
        //Returns the index of the closet vertex
        public static (int,double) GetClosestVertex(List<Point> vertices,Point p)
        {        
            double min = double.MaxValue;
            int id = -1;
            for(int i=0; i<vertices.Count;i++)
            {
                double distance= DistanceToPoint(vertices[i], p);
                if (distance < min)
                {
                    min = distance;
                    id = i;
                }
            }
            double thedistance = DistanceToPoint(vertices[id], p);
            return (id, thedistance);
        }
        //Returns indexes of vertices representing the closest edge
        public static (int,int,double) GetClosestEdge(List<Point> vertices, Point p)
        {
            int id1=-1, id2 = -1;
            double min = double.MaxValue;

            for (int i = 0; i < vertices.Count()-1; i++)
            {
                var d = DistanceToSegment(p,vertices[i], vertices[i + 1]);
                if (d < min)
                {
                    min = d;
                    id1 = i;
                }
            }
            var final = DistanceToSegment(p,vertices[vertices.Count - 1], vertices[0]);
            if (final < min)
            {
                id1 = 0; id2 = vertices.Count - 1;
                min = final;
            }
            else
            {
                id2 = id1 + 1;
            }
            return (id1, id2,min);
        }
        // Calculate the distance between point pt and the segment p1 --> p2.
        public static double DistanceToSegment(Point pt, Point p1, Point p2)
        {
            Point closest;
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Point((int)(p1.X + t * dx),(int)( p1.Y + t * dy));
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}

