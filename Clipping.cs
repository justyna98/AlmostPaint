using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostPaint
{
    class Clipping
    {
        private delegate bool ClippingHandler(float p, float q);
        //Liang-Barsky algorithm
        public void LiangBarsky(Rectangle clip, Point p1, Point p2, Color color)
        {
            int left, right, top, bottom;
            //if rectangle is created top to bottom or the other way
            if (clip.vertices[0].X < clip.vertices[2].X)
            {
                left = clip.vertices[0].X;
                right = clip.vertices[2].X;
                top = clip.vertices[0].Y;
                bottom = clip.vertices[2].Y;
            }
            else
            {
                left = clip.vertices[2].X;
                right = clip.vertices[0].X;
                top = clip.vertices[2].Y;
                bottom = clip.vertices[0].Y;
            }


            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float tE = 0; // tMin
            float tL = 1; // tMax
            ClippingHandler Clip = delegate (float denominator, float numerator)
            {
                if (denominator == 0) //Parallel
                {
                    if (numerator < 0)
                        return false; //outsided - discard
                }
                else
                {
                    float t = numerator / denominator;
                    if (denominator < 0)
                    {
                        if (t > tL) //tE > tL - discard
                            return false;
                        else if (t > tE)
                            tE = t;
                    }
                    else //denom < 0
                    {
                        if (t < tE) //tL < tE - discard
                            return false;
                        else if (t < tL)
                            tL = t;
                    }
                }
                return true;
            };
            if (Clip(-dx, p1.X - left))
            {
                if (Clip(dx, right - p1.X))
                {
                    if (Clip(-dy, p1.Y - top))
                    {
                        if (Clip(dy, bottom - p1.Y))
                        {
                            if (tL < 1) { p2.X = (int)(p1.X + dx * tL); p2.Y = (int)(p1.Y + dy * tL); }
                            if (tE > 0) { p1.X += (int)(dx * tE); p1.Y += (int)(dy * tE); }
                            int thick = 5;
                            bool AA = false;
                            var l = new Line(p1, p2, color, thick, AA);
                            Drawing.DrawLine(l);
                        }
                    }

                }

            }

        }

    }
}
