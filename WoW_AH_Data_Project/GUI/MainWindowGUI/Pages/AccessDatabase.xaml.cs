using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;
using WoWAHDataProject.Database;
using WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI;
using WoWAHDataProject.GUI.DatabaseGUI.ImportToDatabaseGUI;

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

    private async void BtnAccessDatabase_Click(object sender, RoutedEventArgs e)
    {
        AccessDatabaseWindow accessDatabaseWindow = new();
        accessDatabaseWindow.Show();
    }
}
