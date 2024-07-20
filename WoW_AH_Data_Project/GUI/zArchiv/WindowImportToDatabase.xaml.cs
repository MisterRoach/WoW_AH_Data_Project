using System.Windows;
using WoWAHDataProject.Code;

namespace WoWAHDataProject.GUI.DatabaseGUI;

/// <summary>
/// Interaction logic for ImportToDatabaseMainWindow.xaml
/// </summary>
public partial class WindowImportToDatabase : Window
{
    public WindowImportToDatabase()
    {
        InitializeComponent();
        if (Egg.音)
        {
            Egg.Hatch芸術プロ生ちゃんEgg(this);
        }
    }

    private void BtnSelectImportMarketValuesToDatabaseClick(object sender, RoutedEventArgs e)
    {
        WindowImportMarketvaluesToDatabase windowImportMarketvaluesToDatabase = new();
        windowImportMarketvaluesToDatabase.Show();
    }

    private void BtnSelectImportCsvToDatabaseClick(object sender, RoutedEventArgs e)
    {
        WindowImportCsvsToDatabase windowImportCsvsToDatabase = new();
        windowImportCsvsToDatabase.Show();
    }
}
