using System.Windows;
using System.Windows.Controls;
using WoWAHDataProject.GUI.DatabaseGUI;

namespace WoWAHDataProject.GUI.MainWindowGUI.Pages;
/// <summary>
/// Interaction logic for AccessDatabase.xaml
/// </summary>
public partial class AccessDatabase : Page
{
    public AccessDatabase()
    {
        InitializeComponent();
    }
    private void BtnImportToDatabase_Click(object sender, RoutedEventArgs e)
    {
        ImportToDatabaseWindow dbImportWindow = new();
        dbImportWindow.Show();
    }

    private void BtnAccessDatabase_Click(object sender, RoutedEventArgs e)
    {
       // AccessDatabaseWindow accesDbWindow = new AccessDatabaseWindow();
       // accesDbWindow.Show();
    }
}
