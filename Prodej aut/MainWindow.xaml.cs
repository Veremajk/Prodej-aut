using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Prodej_aut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private List<Vuz> NactiVozy(string cesta)
        {
            XDocument dokument = XDocument.Load(cesta);

            List<Vuz> vozy = new List<Vuz>();

            foreach (XElement element in dokument.Descendants("Vuz"))
            {
                Vuz vuz = new Vuz();

                vuz.Model = element.Element("Model").Value;
                vuz.DatumProdeje = DateTime.Parse(element.Element("DatumProdeje").Value);
                vuz.Cena = double.Parse(element.Element("Cena").Value);
                vuz.DPH = double.Parse(element.Element("DPH").Value);

                vozy.Add(vuz);
            }

            return vozy;
        }
        private void VybratXml_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "XML soubory (*.xml)|*.xml|Všechny soubory (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    TextNazevSouboru.Text = "Soubor: " + dialog.SafeFileName;

                    List<Vuz> vozy = NactiVozy(dialog.FileName);

                    List<Vysledek> vysledky = VypocitejVikendoveProdeje(vozy);

                    double celkemBezDph = vysledky.Sum(v => v.CenaBezDph);
                    double celkemSDph = vysledky.Sum(v => v.CenaSDph);

                    TextSouhrn.Text = $"Celkem: {celkemBezDph:N2} Kč bez DPH / {celkemSDph:N2} Kč s DPH";

                    TabulkaVozu.ItemsSource = vozy;

                    TabulkaVysledku.ItemsSource = vysledky;
                }
                catch (Exception chyba)
                {
                    MessageBox.Show(
                        "Nepodařilo se načíst XML soubor.\n\n" + chyba.Message,
                        "Chyba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        private List<Vysledek> VypocitejVikendoveProdeje(List<Vuz> vozy)
        {
            var vysledky = vozy
                .Where(v => v.DatumProdeje.DayOfWeek == DayOfWeek.Saturday ||
                            v.DatumProdeje.DayOfWeek == DayOfWeek.Sunday)
                .GroupBy(v => v.Model)
                .Select(g => new Vysledek
                {
                    Model = g.Key,
                    CenaBezDph = g.Sum(v => v.Cena),
                    CenaSDph = g.Sum(v => v.Cena * (1 + v.DPH / 100))
                })
                .ToList();

            return vysledky;
        }
    }
}