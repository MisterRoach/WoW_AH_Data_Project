using System.Windows;
using System.Windows.Controls;
using WoWAHDataProject.GUI.MainWindowGUI.ViewModels;

namespace WoWAHDataProject.GUI.MainWindowGUI.Pages;
/// <summary>
/// Interaction logic for Database.xaml
/// </summary>
public partial class Database : Page
{
    public Database()
    {
        InitializeComponent();
    }

    private void GoToDatabaseCreationClick(object sender, RoutedEventArgs e)
    {
        MainWindowSubMenuItemsData.NavigateToPage("CreateDatabase");

    }

    private void GoToDatabaseAccessClick(object sender, RoutedEventArgs e)
    {
        MainWindowSubMenuItemsData.NavigateToPage("AccessDatabase");
    }
}
