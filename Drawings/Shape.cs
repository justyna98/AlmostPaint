using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostPaint
{
    abstract public class Shape
    {
        public bool antialiased { get; set; }
        public int Thickness;
        public Color ShapeColor { get; set; }
        public List<Point> Points { get; set; }
        public abstract override string ToString();
        abstract public void Draw();

    }
}
