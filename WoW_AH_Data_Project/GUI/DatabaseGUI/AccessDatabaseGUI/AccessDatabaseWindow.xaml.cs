using System.Data;
using System.Windows;
using Microsoft.Data.Sqlite;
using Serilog;
using WoWAHDataProject.Database;
using WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.ViewModels;

namespace WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI;

public partial class AccessDatabaseWindow : Window
{
    public static SqliteConnection Connection => new(DatabaseMain.connString);

    public AccessDatabaseWindow()
    {
        DataContext = this;
        InitializeComponent();
        AccessDatabaseWindowMenuItemsData.NavigateToPage("AccessDatabaseStartPage");
        this.Closed += WindowClosed;
    }

    public static async Task<AccessDatabaseWindow> CreateAsync()
    {
        AccessDatabaseWindow window = new();
        await InitializeConnectionAsync();
        return window;
    }

    public static async Task InitializeConnectionAsync()
    {
        try
        {
            await Connection.OpenAsync();
            Log.Information("Opened database connection.");
        }
        catch (Exception ex)
        {
            Log.Error("Failed to open database connection.", ex);
        }
    }

    private async void WindowClosed(object sender, EventArgs e)
    {
        try
        {
            if (Connection.State == ConnectionState.Open)
            {
                await Connection.CloseAsync();
                Log.Information("Closed database connection.");
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to close database connection.", ex);
        }
    }
    private void AccessDatabaseWindowFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
    {
    }

    private void AccessDatabaseWindowMenuControl_Loaded(object sender, RoutedEventArgs e)
    {
    }

}