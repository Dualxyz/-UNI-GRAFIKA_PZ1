using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using WpfApp1.Model;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Drawing.Pen;
//using Point = WpfApp1.Model.Point;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Drawing.Size;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //pokupi poziciju klika
        public double poX, poY;
        //za poligon
        public List<double> koordinatePoX = new List<double>();
        public List<double> koordinatePoY = new List<double>();
        //undo redo clear
        List<UIElement> obrisaniListaZaBrojanje = new List<UIElement>();
        List<UIElement> ponovoIscrtaj = new List<UIElement>();
        public int numberChildren = 0;
        //za clear
        List<UIElement> WhatToClear_List = new List<UIElement>();

        //tacke
        List<Point> canvasSpace = new List<Point>();
        public List<PowerEntity> listFromXML = new List<PowerEntity>();
        public List<LineEntity> listVod = new List<LineEntity>();
        public List<LineEntity> listReduntantVod = new List<LineEntity>();
        public Dictionary<Point, PowerEntity> dictPoint = new Dictionary<Point, PowerEntity>();
        //public int MinMaxValue = 1;
        public bool MinMaxValue = true;
        public double noviX, noviY, praviX,praviY, praviXmin, praviXmax, praviYmin, praviYmax;
        public double razlikaMinMaxX, razlikaMinMaxY;
        //za presecanje vodova
        public List<Polyline> listaPresekaVodova = new List<Polyline>();
        public List<Rectangle> listaPresecnihTacaka = new List<Rectangle>();
        
        public MainWindow()
        {
            InitializeComponent();
            Draw_Matrix();
            UcitavanjeElemenata();
        }

        private void Draw_Matrix()
        {
            Point rt;

            for (int i = 0; i < 600; i++)
            {
                for (int j = 0; j < 600; j++)
                {
                    rt = new Point(i, j);
                    canvasSpace.Add(rt);
                }
            }
        }
        private void Draw_Elements()
        {
            foreach (var element in listFromXML)
            {
                ToLatLon(element.X, element.Y, 34, out noviX, out noviY);
                MapToMatrix(noviX, noviY, out praviX, out praviY);


                Rectangle rect = new Rectangle();
                rect.Fill = element.Boja;
                rect.Height = 2;
                rect.Width = 2;
                rect.ToolTip = element.ToolTip;

                Point mojaTacka = canvasSpace.Find(pomocnaTacka => pomocnaTacka.X == praviX && pomocnaTacka.Y == praviY);

                int brojac = 1;
                if (!dictPoint.ContainsKey(mojaTacka))
                {
                    dictPoint.Add(mojaTacka, element);
                }
                else
                {
                    bool flag = false;
                    while (true)
                    {
                        for (double iksevi = praviX - brojac * 3; iksevi <= praviX + brojac * 3; iksevi += 3) //opet na oba 3 da se ne bi preklapali
                        {
                            if (iksevi < 0)
                                continue;
                            for (double ipsiloni = praviY - brojac * 3; ipsiloni <= praviY + brojac * 3; ipsiloni += 3)
                            {
                                if (ipsiloni < 0)
                                    continue;
                                mojaTacka = canvasSpace.Find(t => t.X == iksevi && t.Y == ipsiloni);
                                if (!dictPoint.ContainsKey(mojaTacka))
                                {
                                    dictPoint.Add(mojaTacka, element);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                                break;
                        }
                        if (flag)
                            break;

                        brojac++;
                    }
                }

                Canvas.SetBottom(rect, mojaTacka.X);
                Canvas.SetLeft(rect, mojaTacka.Y);
                canvas.Children.Add(rect);
            }
        }
        private void Draw_Lines()
        {
            foreach (LineEntity line in listVod)
            {
                Point beginning, end;
                findPoints(line, out beginning, out end);

                if (beginning.X != end.X)
                {

                    Polyline polyline = new Polyline();
                    polyline.Stroke = Brushes.Blue;
                    polyline.StrokeThickness = 0.5;


                    Point p1 = new Point(1 + beginning.Y, 600 - 1 - beginning.X);
                    Point p2 = new Point(1 + beginning.Y, 600 - 1 - end.X);
                    Point p3 = new Point(1 + end.Y, 600 - 1 - end.X);
                    polyline.Points.Add(p1);
                    polyline.Points.Add(p2);
                    polyline.Points.Add(p3);
                    polyline.ToolTip = " Name: " + line.Name+ "Line\nID: " + line.Id;

                    polyline.MouseRightButtonDown += functionAnimation; //Stackoverflow moment

                    preklapanjeVodova(p1, p2, p3, polyline);
                    canvas.Children.Add(polyline);
                }
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Draw_Elements();
            Draw_Lines();

            foreach (Polyline pl in listaPresekaVodova)
            {
                if (!canvas.Children.Contains(pl))
                {
                    canvas.Children.Remove(pl);
                }
            }

            numberChildren = canvas.Children.Count; //Za undo i redo da bi clearovao samo stvari koje sam dodao a ne celu mapu
        }

        private void preklapanjeVodova(Point p1,Point p2, Point p3, Polyline polyline)
        {
            Line l1 = new Line();
            Line l2 = new Line();
            l1.X1 = p1.X;
            l1.Y1 = p1.Y;
            l1.X2 = p2.X;
            l1.Y2 = p2.Y;
            l2.X1 = p2.X;
            l2.Y1 = p2.Y;
            l2.X2 = p3.X;
            l2.Y2 = p3.Y;

            l1.Fill = Brushes.DarkRed;
            l2.Fill = Brushes.DarkRed;
            l1.StrokeThickness = 1;
            l1.Stroke = Brushes.DarkRed;
            l2.StrokeThickness = 1;
            l2.Stroke = Brushes.DarkRed;

            foreach (UIElement el in canvas.Children)
            {
                if (el.GetType() == typeof(Polyline))
                {
                    Polyline pl = (Polyline)el;
                    if (pl.Points.Contains(p1) && pl.Points.Contains(p2))
                    {
                        listaPresekaVodova.Add(pl);
                    }
                    if (pl.Points.Contains(p2) && pl.Points.Contains(p3))
                    {
                        listaPresekaVodova.Add(pl);
                    }
                }
            }
        }

        public Queue<(Rectangle, Rectangle)> activeAnim = new Queue<(Rectangle, Rectangle)>(); 
        public bool exists = false;
        private void functionAnimation(object sender, MouseButtonEventArgs e)
        {
            Polyline mojVod = (Polyline)sender;
            Point p1 = new Point();
            Point p2 = new Point();
            p1 = mojVod.Points.First();
            p2 = mojVod.Points.ElementAt(mojVod.Points.Count-1);


            if (!exists)
            {
                Rectangle r = new Rectangle();
                r.Fill = Brushes.Black;
                r.Width = 2;
                r.Height = 2;
                Canvas.SetBottom(r, 600 - 1.5 - p1.Y);
                Canvas.SetLeft(r, -1.5 + p1.X);

                Rectangle r2 = new Rectangle();
                r2.Fill = Brushes.Black; ;
                r2.Width = 2;
                r2.Height = 2;
                Canvas.SetBottom(r2, 600 - 1.5 - p2.Y);
                Canvas.SetLeft(r2, -1.5 + p2.X);

                activeAnim.Enqueue((r, r2));
                canvas.Children.Add(r);
                canvas.Children.Add(r2);
                exists = true;

                Storyboard storyboard = new Storyboard();

                DoubleAnimation animation = new DoubleAnimation();
                animation.To = r.Width * 2;
                animation.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation, r);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Rectangle.WidthProperty));

                storyboard.Children.Add(animation);

                animation = new DoubleAnimation();
                animation.To = r.Height * 2;
                animation.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation, r);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Rectangle.HeightProperty));

                storyboard.Children.Add(animation);
                storyboard.Begin();

                Storyboard storyboard1 = new Storyboard();

                DoubleAnimation animation1 = new DoubleAnimation();
                animation1.To = r2.Width * 2;
                animation1.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation1, r2);
                Storyboard.SetTargetProperty(animation1, new PropertyPath(Rectangle.WidthProperty));

                storyboard1.Children.Add(animation1);

                animation1 = new DoubleAnimation();
                animation1.To = r2.Height * 2;
                animation1.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation1, r2);
                Storyboard.SetTargetProperty(animation1, new PropertyPath(Rectangle.HeightProperty));

                storyboard1.Children.Add(animation1);
                storyboard1.Begin();

            } else
            {
                var (r_del, r2_del) = activeAnim.Dequeue();
                canvas.Children.Remove(r_del);
                canvas.Children.Remove(r2_del);

                Rectangle r = new Rectangle();
                r.Fill = Brushes.Black;
                r.Width = 2;
                r.Height = 2;
                Canvas.SetBottom(r, 600 - 1.5 - p1.Y);
                Canvas.SetLeft(r, -1.5 + p1.X);

                Rectangle r2 = new Rectangle();
                r2.Fill = Brushes.Black;
                r2.Width = 2;
                r2.Height = 2;
                Canvas.SetBottom(r2, 600 - 1.5 - p2.Y);
                Canvas.SetLeft(r2, -1.5 + p2.X);

                activeAnim.Enqueue((r, r2));
                canvas.Children.Add(r);
                canvas.Children.Add(r2);

                Storyboard storyboard = new Storyboard();

                DoubleAnimation animation = new DoubleAnimation();
                animation.To = r.Width * 2;
                animation.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation, r);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Rectangle.WidthProperty));

                storyboard.Children.Add(animation);

                animation = new DoubleAnimation();
                animation.To = r.Height * 2;
                animation.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation, r);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Rectangle.HeightProperty));

                storyboard.Children.Add(animation);
                storyboard.Begin();

                Storyboard storyboard1 = new Storyboard();

                DoubleAnimation animation1 = new DoubleAnimation();
                animation1.To = r2.Width * 2;
                animation1.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation1, r2);
                Storyboard.SetTargetProperty(animation1, new PropertyPath(Rectangle.WidthProperty));

                storyboard1.Children.Add(animation1);

                animation1 = new DoubleAnimation();
                animation1.To = r2.Height * 2;
                animation1.Duration = new Duration(TimeSpan.FromSeconds(1));

                Storyboard.SetTarget(animation1, r2);
                Storyboard.SetTargetProperty(animation1, new PropertyPath(Rectangle.HeightProperty));

                storyboard1.Children.Add(animation1);
                storyboard1.Begin();
            }
        }


        private void findPoints(LineEntity le, out Point beginning, out Point end)
        {
            PowerEntity elem;

            //public Dictionary<Point, PowerEntity> dictPoint = new Dictionary<Point, PowerEntity>();
            elem = listFromXML.Find(x => x.Id == le.FirstEnd);
            beginning = dictPoint.Where(x => x.Value == elem).First().Key;

            elem = listFromXML.Find(x => x.Id == le.SecondEnd);
            end = dictPoint.Where(x => x.Value == elem).First().Key;
        }

        private void CheckXY(double noviX, double noviY)
        {
            if(MinMaxValue)
            {
                praviXmax = noviX;
                praviYmax = noviY;
                praviXmin = noviX;
                praviYmin = noviY;

                MinMaxValue = false;
            }
            else
            {
                if (noviX > praviXmax)
                {
                    praviXmax = noviX;
                }

                if(noviY > praviYmax)
                {
                    praviYmax = noviY;
                }

                if (noviX < praviXmin)
                {
                    praviXmin = noviX;
                }

                if (noviY < praviYmin)
                {
                    praviYmin = noviY;
                }
            }
            razlikaMinMaxX = (praviXmax - praviXmin) *100;
            razlikaMinMaxY = (praviYmax - praviYmin) *100; 
        }

        // daje mi koordinate za canvas
        private void MapToMatrix(double noviX, double noviY, out double praviX, out double praviY)
        {
            double odstojanjeX = 200 / razlikaMinMaxX;
            double odstojanjeY = 200 / razlikaMinMaxY;

            praviX = Math.Round((noviX - praviXmin) * 100 * odstojanjeX);
            praviY = Math.Round((noviY - praviYmin) * 100 * odstojanjeY);

            praviX = praviX * 3;
            praviY = praviY * 3;

        }

        private void UcitavanjeElemenata()
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");
            XmlNodeList nodeList;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                SubstationEntity substationEntity = new SubstationEntity();
                substationEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                substationEntity.Name = node.SelectSingleNode("Name").InnerText;
                substationEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                substationEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                substationEntity.ToolTip = "Substation\nID: " + substationEntity.Id + "  Name: " + substationEntity.Name;
                listFromXML.Add(substationEntity);

                ToLatLon(substationEntity.X, substationEntity.Y, 34, out noviX, out noviY);
                CheckXY(noviX, noviY);
            }


            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity se = new SwitchEntity();
                se.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                se.Name = node.SelectSingleNode("Name").InnerText;
                se.X = double.Parse(node.SelectSingleNode("X").InnerText);
                se.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                se.Status = node.SelectSingleNode("Status").InnerText;
                se.ToolTip = "Switch\nID: " + se.Id + "  Name: " + se.Name + " Status: " + se.Status;
                listFromXML.Add(se);

                ToLatLon(se.X, se.Y, 34, out noviX, out noviY);
                CheckXY(noviX, noviY);
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeEntity = new NodeEntity();
                nodeEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeEntity.Name = node.SelectSingleNode("Name").InnerText;
                nodeEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                nodeEntity.ToolTip = "Node\nID: " + nodeEntity.Id + "  Name: " + nodeEntity.Name;
                listFromXML.Add(nodeEntity);

                ToLatLon(nodeEntity.X, nodeEntity.Y, 34, out noviX, out noviY);
                CheckXY(noviX, noviY);
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                LineEntity lineEntity = new LineEntity();
                lineEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                lineEntity.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    lineEntity.IsUnderground = true;
                }
                else
                {
                    lineEntity.IsUnderground = false;
                }
                lineEntity.R = float.Parse(node.SelectSingleNode("R").InnerText);
                lineEntity.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                lineEntity.LineType = node.SelectSingleNode("LineType").InnerText;
                lineEntity.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                lineEntity.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                lineEntity.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                if (listFromXML.Any(x => x.Id == lineEntity.FirstEnd))
                {
                    if (listFromXML.Any(x => x.Id == lineEntity.SecondEnd))
                    {
                        listVod.Add(lineEntity);
                    }
                }

                //brisanje duplikata
                while (listVod.Any(x => x.Id != lineEntity.Id && x.FirstEnd == lineEntity.FirstEnd && x.SecondEnd == lineEntity.SecondEnd))
                {
                    listReduntantVod = listVod.FindAll(x => x.Id != lineEntity.Id && x.FirstEnd == lineEntity.FirstEnd && x.SecondEnd == lineEntity.SecondEnd);
                    foreach (LineEntity dupli in listReduntantVod)
                    {
                        listVod.Remove(dupli);
                    }
                    listReduntantVod.Clear();
                }
            }
        }


        private void LeviPromeniNesto_Click(object sender, MouseButtonEventArgs e)
        {
            //Update za kliknut objekat
            if (e.OriginalSource is Ellipse)
            {
                Ellipse _clicked = (Ellipse)e.OriginalSource;

                //otvori ovu elipsu
                canvas.Children.Remove(_clicked);

                //za textBlock
                string bojaTeksta = "Black", samTekst = "nekiTekst";
                foreach (FrameworkElement item in canvas.Children)
                {
                    if (item.Name == _clicked.Name + "eltb") //treba mi item.Name, a Name(F12) vodi na FrameworkElement
                    {
                        canvas.Children.Remove(item);
                        bojaTeksta = ((TextBlock)item).Foreground.ToString();
                        samTekst = ((TextBlock)item).Text;
                        break;
                    }
                }

                EditElipsa editElipsa = new EditElipsa(_clicked.Height, _clicked.Width, _clicked.StrokeThickness, _clicked.Fill, bojaTeksta, samTekst);
                editElipsa.Show();

            }
            else if (e.OriginalSource is Polygon)
            {
                Polygon _clicked = (Polygon)e.OriginalSource;

                canvas.Children.Remove(_clicked);

                string bojaTeksta = "Black", samTekst = "nekiTekst";
                foreach (FrameworkElement item in canvas.Children)
                {
                    if (item.Name == _clicked.Name + "pgtb") //treba mi item.Name, a Name(F12) vodi na FrameworkElement
                    {
                        canvas.Children.Remove(item);
                        bojaTeksta = ((TextBlock)item).Foreground.ToString();
                        samTekst = ((TextBlock)item).Text;
                        break;
                    }
                }

                EditPolygon editPoligon = new EditPolygon(_clicked.StrokeThickness, _clicked.Fill.ToString(), bojaTeksta, samTekst, _clicked.Points);
                editPoligon.Show();
            }
            else if (e.OriginalSource is TextBlock)
            {
                TextBlock _clicked = (TextBlock)e.OriginalSource;

                string slova = _clicked.Name;
                slova = slova.Substring(8, slova.Length - 8);
                if (slova != "pgtb" && slova != "eltb")
                {
                    //otvori ovaj tekst
                    canvas.Children.Remove(_clicked);
                    EditText editTekst = new EditText(_clicked.FontSize, _clicked.Foreground, _clicked.Text);
                    editTekst.Show();
                }
            }
        }


        private void LeviPoligon_Click(object sender, MouseButtonEventArgs e)
        {
            Poligon poligonCrtez = new Poligon();
            int i = 1;

            if (EllipseChecked.IsChecked == true && PolygonChecked.IsChecked == true || EllipseChecked.IsChecked == true && TextChecked.IsChecked == true ||
                EllipseChecked.IsChecked == true && PolygonChecked.IsChecked == true && TextChecked.IsChecked == true ||
                PolygonChecked.IsChecked == true && TextChecked.IsChecked == true)
            {
                i = 2;
                MessageBox.Show("Selektujte iskljucivo jednu opciju", "Greska!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (i == 1 && PolygonChecked.IsChecked == true && koordinatePoX.Count >= 3)
            {
                poligonCrtez.Show();
            }
            else if (PolygonChecked.IsChecked == true)
            {
                MessageBox.Show("Click at least 3 times, on 3 various spots", "Error!", MessageBoxButton.OK, MessageBoxImage.Information);
                koordinatePoX.Clear();
                koordinatePoY.Clear();
            }
        }
        private void Right_ClickBiloGde(object sender, MouseButtonEventArgs e)
        {
            int i = 1;

            if(EllipseChecked.IsChecked == true && TextChecked.IsChecked == true)
            {
                i = 2;
                MessageBox.Show("Selektujte iskljucivo jednu opciju", "Greska!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (i == 1)
            {
                if (EllipseChecked.IsChecked == true)
                {
                    Elipsa elipsaCrtez = new Elipsa();
                    poX = Mouse.GetPosition(canvas).X;
                    poY = Mouse.GetPosition(canvas).Y;

                    elipsaCrtez.Show();
                }
                else if (PolygonChecked.IsChecked == true)
                {
                    poX = Mouse.GetPosition(canvas).X;
                    poY = Mouse.GetPosition(canvas).Y;

                    koordinatePoX.Add(poX);
                    koordinatePoY.Add(poY);
                }
                else if (TextChecked.IsChecked == true)
                {
                    AddText dodajTekstCrtez = new AddText();

                    poX = Mouse.GetPosition(canvas).X;
                    poY = Mouse.GetPosition(canvas).Y;

                    dodajTekstCrtez.Show();
                }
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {     
            
            if (canvas.Children.Count > 0)
            {
                obrisaniListaZaBrojanje.Add(canvas.Children[canvas.Children.Count - 1]);
                canvas.Children.Remove(canvas.Children[canvas.Children.Count - 1]);
            }
            
            if (canvas.Children.Count != numberChildren)
            {
                for(int i = 0; i < ponovoIscrtaj.Count; i++)
                {
                    if(ponovoIscrtaj[i] != null)
                        canvas.Children.Add(ponovoIscrtaj[i]);
                }
            }   
            
            for(int i=0; i<ponovoIscrtaj.Count; i++)
            {
                ponovoIscrtaj[i] = null;
            }
        }


        private void Redo_Click(object sender, RoutedEventArgs e)
        {       
            
            if(obrisaniListaZaBrojanje.Count > 0)
            {
                canvas.Children.Add(obrisaniListaZaBrojanje[obrisaniListaZaBrojanje.Count -1]);
                obrisaniListaZaBrojanje.RemoveAt(obrisaniListaZaBrojanje.Count-1);
            }
        }
        
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (canvas.Children.Count > 0)
            {
                foreach(UIElement jedanOdElemenata in canvas.Children)
                {
                    WhatToClear_List.Add(jedanOdElemenata);
                }

                if (canvas.Children.Count > numberChildren)// cuvam one koje zelim da crtam ponovo
                {
                    for(int i = numberChildren; i<canvas.Children.Count; i++)
                    {
                        ponovoIscrtaj.Add(canvas.Children[i]);
                    }
                }
                canvas.Children.Clear();

                for (int i = 0; i < numberChildren; i++)    //Draw the map again
                {
                    canvas.Children.Add(WhatToClear_List[i]);
                }
                WhatToClear_List.Clear();

                numberChildren = canvas.Children.Count;
            }
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
    }
}
