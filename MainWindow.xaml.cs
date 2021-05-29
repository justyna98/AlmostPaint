using System;
using System.Collections.Generic;
using Point = System.Drawing.Point;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using AlmostPaint.Drawings;

namespace AlmostPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            var c1 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)drawingColor.R, (byte)drawingColor.G, (byte)drawingColor.B));
            var c2 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)fillingColor.R, (byte)fillingColor.G, (byte)fillingColor.B));
            var c3 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)clippingColor.R, (byte)clippingColor.G, (byte)clippingColor.B));
            colorbox.Background = c1;
            colorbox2.Background = c2;
            colorbox3.Background = c3;
            image.Source = bitmap;
            Drawing.color = clippingColor;
            Drawing.WriteableBitmap = bitmap;
            TextBox.Text = "- Drawing lines: Midpoint Line\n- Thick lines: Copying pixels\n- Drawing circle: Midpoint circle (Addition)\n- Anti-Alliasing (line and circle): Xiaolin Wu AA\n- Clipping: Liang-Barsky\n- Filling: Scan-Line with Edge Table & BoundaryFill";
            
        }
        System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(255, 0, 0, 0);
        System.Drawing.Color fillingColor = System.Drawing.Color.FromArgb(255, 42, 157, 143);
        System.Drawing.Color clippingColor = System.Drawing.Color.FromArgb(255, 159,17,117);
        
        private WriteableBitmap bitmap = new WriteableBitmap(100, 100, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
        private Point lastClick = new Point();
        private List<Shape> shapes = new List<Shape>();
        private Polygon EditedPolygon = null;
        public bool AA = false;
        private Bitmap fillingPattern = null;
        Clipping clipping = new Clipping();
        List<Point> clickedPoints = new List<Point>();
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.bitmap = new WriteableBitmap((int)image.ActualWidth + 1, (int)image.ActualHeight + 1, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
            image.Source = bitmap;
            Drawing.WriteableBitmap = bitmap;
            Drawing.CleanBitmap();
        }

        #region Save, Load, Reset
        //Save image to json
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Save";
            saveFileDialog1.Filter = "Json files (*.json)|*.json";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                StringBuilder json = new StringBuilder("[");
                for (int i = 0; i < shapes.Count; i++)
                {
                    if (i == shapes.Count() - 1)
                    {
                        json.Append(JsonConvert.SerializeObject(shapes[i], Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore } ));
                    }
                    else {
                        json.Append(JsonConvert.SerializeObject(shapes[i], Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore }) + ", ");
                    }
                    
                }
                json.Append("]");
                File.WriteAllText(saveFileDialog1.FileName, json.ToString()); 
            }

        }
        
        //Load saved files
        private void LoadFile(object sender, RoutedEventArgs e)
        {
            var filePath = string.Empty;
            shapes = new List<Shape>();
            List<Shape> listshapes = new List<Shape>();
            Drawing.CleanBitmap();

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Json files(*.json)| *.json";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string json = reader.ReadToEnd();

                        listshapes = JsonConvert.DeserializeObject<List<Shape>>(json, new Newtonsoft.Json.JsonSerializerSettings
                        {
                            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                        });
                    }
                
                }
            }
            foreach(var shape in listshapes)
            {
                shapes.Add(shape);
            }
            Drawing.Redraw(listshapes);
        }
        private void Rest(object sender, RoutedEventArgs e)
        {
            shapes = new List<Shape>();
            Drawing.CleanBitmap();
            clickedPoints.Clear();

        }
        #endregion
        
        #region Instructions
        //Print instructions and X and Y coordinates
        private void Image_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = Mouse.GetPosition(image);
            LocatioinBox.Text = "X: "+position.X.ToString()+" Y: "+ position.Y.ToString();


            if (ChosenLine.IsChecked == true)
            {
                TextBox.Text = "Drawing line: \n- Click to pick the start, click again to pick the end of the line";
            }
            if (ChosenCircle.IsChecked == true)
            {
                TextBox.Text = "Drawing circle: \n- Click to pick the origin, click again to pick the opposite radius";
            }
            if (ChosenPoly.IsChecked == true)
            {
                TextBox.Text = "Drawing polygon: \n- Click to pick the start vertex, add vertices by clicking, finish the polygon by clicking near the start vertex.\n- Requirement: Must have at least 3 vertices";
            }
            if (ChosenRectangle.IsChecked == true)
            {
                TextBox.Text = "Drawing rectangle: \n- Click to pick the starting corner, click again to pick the opposite corner ";
            }
            if (movinggbutton.IsChecked == true)
            {
                TextBox.Text = "All shapes\n- Moving line: click on the line  and relase on the desired position\n- Moving circle: Click on the circumference and release on the desired origin\n- Moving polygon: Click near vertex and choose desired place for that vertex\n-Moving rectangle: Click on the circumference and release on the desired position";
            }
            if (Eradiussbutton.IsChecked == true)
            {
                TextBox.Text = "Circle\n- Changing radius: Click on the circumference and release on the desired radius";
            }
            if (deletingbutton.IsChecked == true)
            {
                TextBox.Text = "All shapes\n- Click on the shape to delete it";
            }
            if (Ethickbutton.IsChecked == true)
            {
                TextBox.Text = "Line\\Polygon\\Rectangle\n- Click on the shape to change the thickness to currently chosen thickness\n- Turn off Anti-Aliasing to see the results";
            }
            if (Ecolorbutton.IsChecked == true)
            {
                TextBox.Text = "All shapes\n- Click on the shape to change the color to currently chosen color";
            }
            if (Eedgsbutton.IsChecked == true)
            {
                TextBox.Text = "Polygon\\Rectangle\n- Click on the edge and drag it to the desired position";
            }
            if (Everticesbutton.IsChecked == true)
            {
                TextBox.Text = "Line\\Polygon\\Rectangle\n- Click on the shape's vertex to change its position";
            }
            if(ClipToRec.IsChecked == true)
            {
                TextBox.Text = "Line\\Polygon\\Rectangle\n- First click on the rectangle, and then the shape to be clipped";
            }
            if (FillCpoly.IsChecked == true)
            {
                TextBox.Text = "Polygon\\Rectangle\n- Click on the circumference of the shape to be filled";
            }
            if (FillIpoly.IsChecked == true)
            {
                TextBox.Text = "Polygon\\Rectangle\n- Choose the pattern and click on the circumference of the shape to be filled";
            }
            if (BoundaryFill.IsChecked == true)
            {
                TextBox.Text = "All shapes\n- click on the area to fill it \n- BoundaryFill: the boundary color is the DrawingColor";
            }
        }
        #endregion

        #region Changing colors and patterns
        //Change drawing color 
        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            ColorDialog dig = new ColorDialog();
            if (dig.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                drawingColor = System.Drawing.Color.FromArgb(dig.Color.A, dig.Color.R, dig.Color.G, dig.Color.B);
                colorbox.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B));

            }
        }
        private void ChangeFillingColor(object sender, RoutedEventArgs e)
        {
            ColorDialog dig = new ColorDialog();
            if (dig.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fillingColor = System.Drawing.Color.FromArgb(dig.Color.A, dig.Color.R, dig.Color.G, dig.Color.B);
                colorbox2.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(fillingColor.A, fillingColor.R, fillingColor.G, fillingColor.B));

            }
        }

        private void ChangeClippingColor(object sender, RoutedEventArgs e)
        {
            ColorDialog dig = new ColorDialog();
            if (dig.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                clippingColor = System.Drawing.Color.FromArgb(dig.Color.A, dig.Color.R, dig.Color.G, dig.Color.B);
                colorbox3.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(clippingColor.A, clippingColor.R, clippingColor.G, clippingColor.B));
                Drawing.color = clippingColor;
            }
        }

        private void ChoosePattern(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*bmp|" +
              "JPEG|*.jpg;*.jpeg|" +
              "Bitmap|*.bmp";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fillingPattern = new System.Drawing.Bitmap(op.FileName);
            }
            ChoosePatternButton.Background = new ImageBrush(Convert(fillingPattern));
            ChoosePatternButton.Content = " ";
        }
        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
        #endregion

        // Controllers
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition(image).X;
            int y = (int)e.GetPosition(image).Y;
            Point currentPoint = new Point(x, y);
            //Drawing shapes

            //drawing line
            if (ChosenLine.IsChecked == true)
            {
                int thick = Int32.Parse(ThicknessBox.SelectionBoxItem.ToString());
                    
                if (clickedPoints.Count == 1)
                {
                    clickedPoints.Add(lastClick);
                    var l = new Line(clickedPoints[0], clickedPoints[1], drawingColor, thick, AA);
                    l.Draw();
                    shapes.Add(l);
                    clickedPoints.Clear();
                }
                else
                {
                    clickedPoints.Add(lastClick);
                }
            }
            //drawing circle;
            if (ChosenCircle.IsChecked == true)
            {
                if (clickedPoints.Count == 1)
                {
                    clickedPoints.Add(lastClick);
                    int radius = (int)Drawing.DistanceToPoint(clickedPoints[0], clickedPoints[1]);
                    var c = new Circle(clickedPoints[0], radius, drawingColor, AA);
                    c.Draw();
                    shapes.Add(c);
                    clickedPoints.Clear();
                }
                else
                {
                    clickedPoints.Add(lastClick);
                }
            }
            //drawing polygon
            if (ChosenPoly.IsChecked == true)
            {
                int thick = Int32.Parse(ThicknessBox.SelectionBoxItem.ToString());
                    
                if (Drawing.PolyFirstPoint == null)
                {
                    var l = new Line(lastClick, currentPoint, drawingColor, thick,AA);
                    l.Draw();
                    Drawing.PolyFirstPoint = currentPoint;
                    EditedPolygon = new Polygon(drawingColor, thick,AA);
                    EditedPolygon.Add(lastClick);

                }
                else
                {
                    var d1 = Drawing.DistanceToPoint(EditedPolygon.vertices[0], currentPoint);
                    if (d1 < 10 && EditedPolygon.vertices.Count()>=3) {
                        var ll = new Line(EditedPolygon.vertices[EditedPolygon.vertices.Count() - 1], EditedPolygon.vertices[0], drawingColor, thick,AA);
                        ll.Draw();
                        EditedPolygon.Draw();
                        shapes.Add(EditedPolygon);
                        EditedPolygon = null;
                        Drawing.PolyFirstPoint = null;

                    }
                    else
                    {

                        var ll = new Line(lastClick, EditedPolygon.vertices[EditedPolygon.vertices.Count() - 1], drawingColor, thick,AA);
                        ll.Draw();
                        EditedPolygon.Add(currentPoint);

                    }

                }

            }
            //drawing sector
            if (ChosenSector.IsChecked == true)
            {
                if (clickedPoints.Count == 2)
                {
                    clickedPoints.Add(lastClick);
                    Sector s = new Sector(clickedPoints[0], clickedPoints[1], clickedPoints[2], drawingColor);
                    s.Draw();
                    clickedPoints.Clear();
                    shapes.Add(s);
                }
                else
                {
                    clickedPoints.Add(lastClick);
                }
            }
            // drawing rectangle
            if (ChosenRectangle.IsChecked == true)
            {
                int thick = Int32.Parse(ThicknessBox.SelectionBoxItem.ToString());
                if (clickedPoints.Count == 1)
                {
                    clickedPoints.Add(lastClick);
                    Rectangle r = new Rectangle(clickedPoints[0], clickedPoints[1], drawingColor, thick, AA);
                    r.Draw();
                    shapes.Add(r);
                    clickedPoints.Clear();
                }
                else
                {
                    clickedPoints.Add(lastClick);
                }

            }
            //Moving shapes
            if (movinggbutton.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);

                if (shapetoEdit != null)
                {
                    //moving circle
                    if (shapetoEdit is Circle)
                    {
                        var cir = shapetoEdit as Circle;
                        cir.origin.X = x;
                        cir.origin.Y = y;
                        Drawing.Redraw(shapes);

                    }
                    //moving line
                    if (shapetoEdit is Line)
                    {
                        var l = shapetoEdit as Line;
                        var dx = lastClick.X - currentPoint.X;
                        var dy = lastClick.Y - currentPoint.Y;


                        l.startPoint = new Point(l.startPoint.X - dx, l.startPoint.Y - dy);
                        l.endPoint = new Point(l.endPoint.X - dx, l.endPoint.Y - dy);
                        Drawing.Redraw(shapes);

                    }
                    //moving polygon
                    if(shapetoEdit is Polygon)
                    {
                        var p = shapetoEdit as Polygon;
                        var temp = Drawing.GetClosestVertex(p.vertices, lastClick);
                        var v = temp.Item1;
                        var d = temp.Item2;
                        int dx=0, dy=0;
                        for (int i = 0; i < p.vertices.Count; i++)
                        {
                            if (v == i)
                            {
                                dx = p.vertices[i].X - currentPoint.X;
                                dy = p.vertices[i].Y - currentPoint.Y;

                            }
                        }
                        for (int i = 0; i < p.vertices.Count; i++)
                        {
                            Point pp = new Point(p.vertices[i].X - dx, p.vertices[i].Y - dy);
                            p.vertices[i] = pp;
                        }
                        Drawing.Redraw(shapes);
                    }
                    //moving rectangle
                    if (shapetoEdit is Rectangle)
                    {
                        var r = shapetoEdit as Rectangle;
                        var dx = lastClick.X - currentPoint.X;
                        var dy = lastClick.Y - currentPoint.Y;
                        for(int i = 0; i < r.vertices.Count; i++)
                        {
                            r.vertices[i]= new Point(r.vertices[i].X - dx, r.vertices[i].Y - dy);
                        }
                        Drawing.Redraw(shapes);

                    }
                }
            }
            //Editing radius - only for circles
            if (Eradiussbutton.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);

                if (shapetoEdit != null)
                {
                    if (shapetoEdit is Circle)
                    {
                        var cir = shapetoEdit as Circle;
                        var d = Drawing.DistanceToPoint(cir.origin.X, cir.origin.Y, x,y);
                        cir.radius = (int)d;
                        Drawing.Redraw(shapes);

                    }
                }
            }
            //Editing vertices
            if (Everticesbutton.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);

                if (shapetoEdit != null)
                {
                    //moving endpoints of line
                    if (shapetoEdit is Line)
                    {
                        var l = shapetoEdit as Line;
                        var d1 = Drawing.DistanceToPoint(l.startPoint.X, l.startPoint.Y, lastClick.X, lastClick.Y);
                        var d2 = Drawing.DistanceToPoint(l.endPoint.X, l.endPoint.Y, lastClick.X, lastClick.Y);

                        if (d1 < 10)
                        {
                            l.startPoint.X = x;
                            l.startPoint.Y = y;
                            Drawing.Redraw(shapes);

                        }
                        if (d2 < 10)
                        {
                            l.endPoint.X = x;
                            l.endPoint.Y = y;
                            Drawing.Redraw(shapes);

                        }
                    }
                    if(shapetoEdit is Polygon)
                    {
                        var p = shapetoEdit as Polygon;
                        var temp = Drawing.GetClosestVertex(p.vertices, lastClick);
                        var v = temp.Item1;
                        var d = temp.Item2;
                        if (d < 10)
                        {
                            for(int i=0; i < p.vertices.Count; i++)
                            {
                                if (v == i)
                                {
                                    p.vertices[i] = currentPoint;
                                    Drawing.Redraw(shapes);
                                }
                            }
                        }

                    }
                    if (shapetoEdit is Rectangle)
                    {
                        var r = shapetoEdit as Rectangle;
                        var yeet = Drawing.GetClosestVertex(r.vertices, currentPoint);
                        var v2 = yeet.Item1;
                        int v1, v3;

                        if (v2 == 0)
                        {
                            v1 = 3;
                            v3 = 1;
                        }
                        else if (v2 == 3)
                        {
                            v1 = 2;
                            v3 = 0;
                        }
                        else
                        {
                            v1 = v2-1;
                            v3 = v2+1;
                        }
                        var dx = lastClick.X - currentPoint.X;
                        var dy = lastClick.Y - currentPoint.Y;
                        r.vertices[v2] = currentPoint;
                        if (v2 == 1 || v2 == 3)
                        {
                            r.vertices[v1] = new Point(r.vertices[v1].X, currentPoint.Y);
                            r.vertices[v3] = new Point(currentPoint.X, r.vertices[v3].Y);
                        }
                        else
                        {
                            r.vertices[v1] = new Point(currentPoint.X, r.vertices[v1].Y);
                            r.vertices[v3] = new Point(r.vertices[v3].X, currentPoint.Y);
                        }
                        Drawing.Redraw(shapes);

                    }

                }
            }
            //Moving edges
            if (Eedgsbutton.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);

                if (shapetoEdit != null)
                {
                    if (shapetoEdit is Polygon)
                    {
                        var p = shapetoEdit as Polygon;
                        var yeet = Drawing.GetClosestEdge(p.vertices, currentPoint);
                        var dx = lastClick.X - currentPoint.X;
                        var dy = lastClick.Y - currentPoint.Y;


                        p.vertices[yeet.Item1] = new Point(p.vertices[yeet.Item1].X-dx, p.vertices[yeet.Item1].Y - dy);
                        p.vertices[yeet.Item2] = new Point(p.vertices[yeet.Item2].X - dx, p.vertices[yeet.Item2].Y - dy);
                        Drawing.Redraw(shapes);
                    }
                    if (shapetoEdit is Rectangle)
                    {
                        var r = shapetoEdit as Rectangle;
                        var yeet = Drawing.GetClosestEdge(r.vertices, currentPoint);
                        var v1 = yeet.Item1;
                        var v2 = yeet.Item2;

                        var dx = lastClick.X - currentPoint.X;
                        var dy = lastClick.Y - currentPoint.Y;

                        if (Math.Abs(dx) > Math.Abs(dy))
                        {
                            r.vertices[v1] = new Point(currentPoint.X, r.vertices[v1].Y) ;
                            r.vertices[v2] = new Point(currentPoint.X, r.vertices[v2].Y);
                        }
                        else
                        {
                            r.vertices[v1] = new Point(r.vertices[v1].X, currentPoint.Y);
                            r.vertices[v2] = new Point(r.vertices[v2].X, currentPoint.Y);
                        }
                        Drawing.Redraw(shapes);
                        

                    }
                }
            }
            //Deleting
            if (deletingbutton.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);
                if (shapetoEdit != null)
                {
                    shapes.Remove(shapetoEdit);
                    Drawing.Redraw(shapes);
                }
            }
            //Changing thickness
            if (Ethickbutton.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);
                if (shapetoEdit != null)
                {
                    if (shapetoEdit is Line)
                    {
                        var l = shapetoEdit as Line;
                        l.Thickness = Int32.Parse(ThicknessBox.SelectionBoxItem.ToString());
                        Drawing.Redraw(shapes);
                    }
                    if(shapetoEdit is Polygon)
                    {
                        var p = shapetoEdit as Polygon;
                        p.Thickness= Int32.Parse(ThicknessBox.SelectionBoxItem.ToString());
                        Drawing.Redraw(shapes);
                    }
                    if (shapetoEdit is Rectangle)
                    {
                        var r = shapetoEdit as Rectangle;
                        r.Thickness = Int32.Parse(ThicknessBox.SelectionBoxItem.ToString());
                        Drawing.Redraw(shapes);

                    }
                }

            }
            //Changing color
            if (Ecolorbutton.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);
                if (shapetoEdit != null)
                {
                    if (shapetoEdit is Line)
                    {
                        var l = shapetoEdit as Line;
                        l.ShapeColor = drawingColor;
                        Drawing.Redraw(shapes);
                    }
                    if (shapetoEdit is Circle)
                    {
                        var c = shapetoEdit as Circle;
                        c.ShapeColor = drawingColor;
                        Drawing.Redraw(shapes);
                    }
                    if (shapetoEdit is Polygon)
                    {
                        var p = shapetoEdit as Polygon;
                        p.ShapeColor = drawingColor;
                        Drawing.Redraw(shapes);
                    }
                    if (shapetoEdit is Rectangle)
                    {
                        var r = shapetoEdit as Rectangle;
                        r.ShapeColor = drawingColor;
                        Drawing.Redraw(shapes);

                    }

                }

            }
            //Filling with color
            if (FillCpoly.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);

                if (shapetoEdit != null)
                {
                    if (shapetoEdit is Polygon)
                    {
                        var poly = shapetoEdit as Polygon;
                        poly.FillColor = fillingColor;
                        poly.FillPattern = null;
                        poly.Draw();
                    }
                    if (shapetoEdit is Rectangle)
                    {
                        var rect = shapetoEdit as Rectangle;
                        rect.FillColor = fillingColor;
                        rect.FillPattern = null;
                        rect.Draw();
                    }
                }
            }
            //Filling with image
            if (FillIpoly.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);
                if (fillingPattern == null)
                    System.Windows.MessageBox.Show("Choose pattern");
                if (shapetoEdit != null)
                {
                    if (shapetoEdit is Polygon)
                    {
                        var poly = shapetoEdit as Polygon;
                        poly.FillColor = null;
                        poly.FillPattern = fillingPattern;
                        poly.Draw();
                    }
                    if (shapetoEdit is Rectangle)
                    {
                        var rect = shapetoEdit as Rectangle;
                        rect.FillColor = null;
                        rect.FillPattern = fillingPattern;
                        rect.Draw();
                    }
                }
            }
            //Clipping to rectangle
            if (ClipToRec.IsChecked == true)
            {
                var shapetoEdit = Drawing.GetShape(shapes, lastClick.X, lastClick.Y);
                if (Drawing.ClippingShape == null)
                {
                    if (shapetoEdit is Rectangle)
                    {
                        Drawing.ClippingShape = shapetoEdit as Rectangle;
                    }
                }
                else
                {
                    Drawing.clipping = true;
                    if (!shapetoEdit.Equals(Drawing.ClippingShape))
                    {
                        Drawing.ClippedShape = shapetoEdit;
                        if (shapetoEdit is Line)
                        {
                            var l = shapetoEdit as Line;
                            clipping.LiangBarsky(Drawing.ClippingShape, l.startPoint, l.endPoint, clippingColor);

                        }
                        if (shapetoEdit is Rectangle)
                        {
                            var rect = shapetoEdit as Rectangle;
                            var vertices= rect.vertices;
                            for(int i = 0; i <vertices.Count() - 1; i++)
                            {
                                clipping.LiangBarsky(Drawing.ClippingShape, vertices[i], vertices[i + 1], clippingColor);
                            }
                            clipping.LiangBarsky(Drawing.ClippingShape, vertices[0], vertices[vertices.Count - 1], clippingColor);
                        }
                        if (shapetoEdit is Polygon)
                        {
                            var poly = shapetoEdit as Polygon;
                            var vertices =poly.vertices;
                            for (int i = 0; i < vertices.Count() - 1; i++)
                            {
                                clipping.LiangBarsky(Drawing.ClippingShape, vertices[i], vertices[i + 1], clippingColor);
                            }
                            clipping.LiangBarsky(Drawing.ClippingShape, vertices[0], vertices[vertices.Count - 1], clippingColor);
                        }
                    }
                }
            }
            //BoundaryFill
            if (BoundaryFill.IsChecked == true)
            {
                Filling.boundaryFill(lastClick.X, lastClick.Y, fillingColor, drawingColor);
            }

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastClick.X= (int)e.GetPosition(image).X;
            lastClick.Y = (int)e.GetPosition(image).Y;
        }

        // AA - check
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AA = true;
            foreach(var shape in shapes)
            {
                shape.antialiased = true;
            }
            Drawing.Redraw(shapes);
        }

        //AA - uncheck
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AA = false;
            foreach (var shape in shapes)
            {
                shape.antialiased = false;
            }
            Drawing.Redraw(shapes);
        }
    }
}
