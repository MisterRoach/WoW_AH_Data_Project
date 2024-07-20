using System.Windows.Controls;
using WoWAHDataProject.Database;

namespace WoWAHDataProject.GUI.MainWindowGUI.Pages;

public partial class Database : Page
{
    public Database()
    {
        InitializeComponent();
    }

    private void CreateDatabaseBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DatabaseMain.DataBaseMain();
    }
}
