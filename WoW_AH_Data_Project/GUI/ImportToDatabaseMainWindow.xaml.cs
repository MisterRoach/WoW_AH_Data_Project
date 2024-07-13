using System.Windows;
using WoWAHDataProject.Code;

namespace WoWAHDataProject.GUI
{
    /// <summary>
    /// Interaction logic for ImportToDatabaseMainWindow.xaml
    /// </summary>
    public partial class ImportToDatabaseMainWindow : Window
    {
        public ImportToDatabaseMainWindow()
        {
            InitializeComponent();
            if (Egg.音)
            {
                Egg.Hatch芸術プロ生ちゃんEgg(this);
            }
        }

        private void BtnSelectImportMarketValuesToDatabaseClick(object sender, RoutedEventArgs e)
        {
            ImportMarketValuesToDatabaseWindow importMarketValuesToDatabaseWindow = new();
            importMarketValuesToDatabaseWindow.Show();
        }

        private void BtnSelectImportCsvToDatabaseClick(object sender, RoutedEventArgs e)
        {
            ImportCsvsToDatabaseWindow importCsvsToDatabaseWindow = new();
            importCsvsToDatabaseWindow.Show();
        }
    }
}
