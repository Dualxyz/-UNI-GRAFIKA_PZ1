using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Poligon.xaml
    /// </summary>
    public partial class Poligon : Window
    {
        public Poligon()
        {
            InitializeComponent();

            cmbColor.ItemsSource = typeof(Colors).GetProperties();
            cmbColor2.ItemsSource = typeof(Colors).GetProperties();
        }

        private void NacrtajPoligon_Click(object sender, RoutedEventArgs e)
        {
            double debljinaKonturneLinijeee = 1, LineThicknessParse;
            if (validate())
            {
                if (!double.TryParse(debljinaKonturneLinije.Text, out LineThicknessParse)) { }
                debljinaKonturneLinijeee = LineThicknessParse;

                Polygon polygon = new Polygon();
                polygon.StrokeThickness = debljinaKonturneLinijeee;
                polygon.Stroke = Brushes.Black;

                Canvas.SetZIndex(polygon, 3);

                string boja = cmbColor.SelectedItem.ToString();
                // dobijem System.Windows.Media.Color Blue -> 27 viska
                boja = boja.Substring(27, boja.Length - 27);
                //tekstUnutarElipse.Text = boja;
                polygon.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boja));
                //polygon.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boja));


                double koordinataX = 0, koordinataY = 0;
                double poXmin = 0, poXmax = 0, poYmin = 0, poYmax = 0, prolaz = 0;
                int brojZaUslov = ((MainWindow)Application.Current.MainWindow).koordinatePoX.Count;
                for (int i = 0; i < brojZaUslov; i++)
                {
                    koordinataX = ((MainWindow)Application.Current.MainWindow).koordinatePoX[i];
                    koordinataY = ((MainWindow)Application.Current.MainWindow).koordinatePoY[i];

                    System.Windows.Point tacka = new System.Windows.Point(koordinataX, koordinataY);

                    if (prolaz == 0)
                    {
                        poXmax = koordinataX;
                        poXmin = koordinataX;
                        poYmax = koordinataY;
                        poYmin = koordinataY;
                        prolaz++;
                    }
                    else
                    {
                        //CheckXY X
                        if (koordinataX > poXmax)
                        {
                            poXmax = koordinataX;
                        }

                        if (koordinataX < poXmin)
                        {
                            poXmin = koordinataX;
                        }

                        //CheckXY Y 
                        if (koordinataY > poYmax)
                        {
                            poYmax = koordinataY;
                        }

                        if (koordinataY < poYmin)
                        {
                            poYmin = koordinataY;
                        }
                    }

                    polygon.Points.Add(tacka);
                }



                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                var stringChars = new char[8];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);
                polygon.Name = finalString;
                TextBlock prosledjujemTekst = new TextBlock();
                prosledjujemTekst.Margin = new Thickness(koordinataX - (poXmax - poXmin) / 2, koordinataY - (poYmax - poYmin) / 2, 0, 0);
                prosledjujemTekst.FontSize = 10;

                Canvas.SetZIndex(prosledjujemTekst, 4);

                string tekstZaPoligon = "";
                if (tekstUnutarPoligona.Text.Length != 0)
                {
                    tekstZaPoligon = tekstUnutarPoligona.ToString();
                    tekstZaPoligon = tekstZaPoligon.Substring(33, tekstZaPoligon.Length - 33);
                }

                if (cmbColor2.SelectedItem != null)
                {
                    string boja2 = cmbColor2.SelectedItem.ToString();
                    boja2 = boja2.Substring(27, boja2.Length - 27);
                    prosledjujemTekst.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(boja2));
                }

                prosledjujemTekst.Text = tekstZaPoligon;
                prosledjujemTekst.Name = finalString + "pgtb";

                ((MainWindow)Application.Current.MainWindow).PolygonChecked.IsChecked = false;

                ((MainWindow)Application.Current.MainWindow).canvas.Children.Add(prosledjujemTekst);
                ((MainWindow)Application.Current.MainWindow).canvas.Children.Add(polygon);

                ((MainWindow)Application.Current.MainWindow).koordinatePoX.Clear();
                ((MainWindow)Application.Current.MainWindow).koordinatePoY.Clear();
                this.Close();
            }
            else
            {
                MessageBox.Show("Fill in all the mandatory fields", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool validate()
        {
            bool result = true;


            double LineThicknessParse;

            if (debljinaKonturneLinije.Text.Trim().Equals(""))
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "Thickness cannot be empty";
            }
            else if (debljinaKonturneLinije.Text.Length > 10)
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "Number is too big";
            }
            else if (!double.TryParse(debljinaKonturneLinije.Text, out LineThicknessParse))
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "No characters allowed";
            }
            else if (LineThicknessParse <= 0)
            {
                result = false;
                debljinaKonturneLinije.BorderBrush = Brushes.Red;
                debljinaKonturneLinije.BorderThickness = new Thickness(1);
                lblKonturnaLinijaGreska.Content = "Needs to be higher than 0";
            }
            else
            {
                debljinaKonturneLinije.BorderBrush = Brushes.Green;
                lblKonturnaLinijaGreska.Content = string.Empty;
            }

            //boja
            if (cmbColor.SelectedItem == null)
            {
                result = false;
                cmbColor.BorderBrush = Brushes.Red;
                cmbColor.BorderThickness = new Thickness(1);
                lblcmdGreska.Content = "Need to select a color";
            }
            else
            {
                cmbColor.BorderBrush = Brushes.Green;
                lblcmdGreska.Content = string.Empty;
            }

            return result;
        }

        private void cmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbColor.SelectedItem != null)
            {
                try
                {
                    var izabrana = (PropertyInfo)cmbColor.SelectedItem;
                    var boja = (Color)izabrana.GetValue(null, null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Color isn't selected", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
