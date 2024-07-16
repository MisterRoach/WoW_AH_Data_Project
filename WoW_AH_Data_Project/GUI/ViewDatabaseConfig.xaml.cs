using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Forms = System.Windows.Forms;
namespace WoWAHDataProject.GUI;

/// <summary>
/// Interaction logic for ViewDatabaseConfig.xaml
/// </summary>
public partial class ViewDatabaseConfig : Window
{
    private static SqliteConnection connection;
    private ObservableCollection<View> viewCollection = new ObservableCollection<View>();
    private ViewDatabaseConfig(SqliteConnection sqliteConnection)
    {
        InitializeComponent();
        connection = sqliteConnection;
        this.Closed += (sender, e) => WindowClosed(sender, e);
    }

    public static async Task<ViewDatabaseConfig> CreateAsync(SqliteConnection connection)
    {
        ViewDatabaseConfig window = new ViewDatabaseConfig(connection);
        await InitializeConnectionAsync();
        return window;
    }

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
    public void ShowLoadingIndicator(bool show)
    {
        loadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }
    private async void BtnSelectTable_Click(object sender, RoutedEventArgs e)
    {
        ShowLoadingIndicator(true);
        try
        {
            List<string> columnSelectionList = new List<string>();
            var content = ListViewTable.Items.SourceCollection;
            foreach (var entry in content.Cast<View>().ToList())
            {
                if (entry.IsChecked)
                {
                    columnSelectionList.Add(entry.ColumnName);
                }
            }

            ViewDatabaseTableWindow viewDatabaseTable = new ViewDatabaseTableWindow(DatabaseComboBox.SelectedItem.ToString(), columnSelectionList, connection);
            viewDatabaseTable.Show();
            await viewDatabaseTable.LoadDataAsync();
        }
        finally
        {
            ShowLoadingIndicator(false);
        }
    }
    private void DatabaseComboBoxDropDownOpened(object sender, EventArgs e)
    {
        SqliteCommand countCommand = new SqliteCommand("SELECT COUNT(name) FROM sqlite_master WHERE type='table';", connection);
        SqliteCommand selectCommand = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection);
        SqliteDataReader count = countCommand.ExecuteReader();
        count.Read();
        SqliteDataReader selectReader = selectCommand.ExecuteReader();
        while (selectReader.Read() && DatabaseComboBox.Items.Count < Int32.Parse(count.GetValue(0).ToString(), CultureInfo.CurrentCulture))
        {
            DatabaseComboBox.Items.Add(selectReader.GetString(0));
        }
        DatabaseComboBox.Items.Refresh();
    }
    private void DatabaseComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        GetTableColumns(DatabaseComboBox.SelectedItem.ToString());
        if(ListViewTable.Visibility == Visibility.Hidden)
        {
            ListViewTable.Visibility = Visibility.Visible;
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
            viewCollection.Add(new View { IsChecked = false, ColumnName = reader.GetString(0) });
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
    private void GridViewColumn_Unchecked(object sender, RoutedEventArgs e)
    {

    }

    private void CheckBoxIncludeInResultChecked(object sender, RoutedEventArgs e)
    {

    }

    private void CheckBoxIncludeInResultUnchecked(object sender, RoutedEventArgs e)
    {

    }
}
