using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
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
using Microsoft.Data.Sqlite;
using Serilog;

namespace WoWAHDataProject.GUI.DatabaseGUI;
/// <summary>
/// Interaction logic for AccessDatabaseWindow.xaml
/// </summary>
public partial class ImportToDatabaseWindow : Window
{
    private static SqliteConnection connection;
    private ObservableCollection<ComponentTrackListitemsState> viewCollection = new ObservableCollection<ComponentTrackListitemsState>();
    public ImportToDatabaseWindow()
    {
        InitializeComponent();
        connection = new SqliteConnection(Database.DatabaseMain.connString);
        this.Closed += (sender, e) => WindowClosed(sender, e);
    }

    /*public static async Task<AccessDatabaseWindow> CreateAsync(SqliteConnection connection)
    {
        AccessDatabaseWindow window = new AccessDatabaseWindow(connection);
        await InitializeConnectionAsync();
        return window;
    }*/

    public static async Task InitializeConnectionAsync()
    {
        try
        {
            await connection.OpenAsync();
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
            if (connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
                Log.Information("Closed database connection.");
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to close database connection.", ex);
        }
    }

    private void GetTableColumns(string tableName)
    {
        viewCollection.Clear();
        SqliteCommand command = new SqliteCommand($"SELECT name FROM pragma_table_info('{tableName}');", connection);
        SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Log.Information(reader.GetString(0));
            viewCollection.Add(new ComponentTrackListitemsState { IsChecked = false, ColumnName = reader.GetString(0) });
        }
        ListViewTable.ItemsSource = viewCollection;
        ResizeGridViewColumn(GridViewColumnColumns);
        ListViewTable.Items.Refresh();
    }

    private static void ResizeGridViewColumn(GridViewColumn column)
    {
        if (double.IsNaN(column.Width))
        {
            column.Width = column.ActualWidth;
        }
        column.Width = double.NaN;
    }
}
