using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;
using Serilog;
using WoWAHDataProject.Code;
using WoWAHDataProject.Database;
using Forms = System.Windows.Forms;

namespace WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.Pages;

/// <summary>
/// Interaction logic for ViewTablePage.xaml
/// </summary>
public partial class ViewTablePage : Page, IDisposable
{
    private static readonly SqliteConnection connection = new(DatabaseMain.connString);
    private static int dataOffset;
    private static string limitString;
    private static readonly List<string> columnSelectionList = AccessDatabaseStartPage.columnSelectionList;
    private readonly DataTable dataTable;

    public ViewTablePage()
    {
        connection.Open();
        dataTable = new DataTable(AccessDatabaseStartPage.SelectedTable);
        InitializeComponent();
        if (AccessDatabaseStartPage.SelectedRowAmount is "All" or null or "Amount of rows to display/load at once")
        {
            limitString = "";
        }
        else if (AccessDatabaseStartPage.SelectedRowAmount != null)
        {
            limitString = $"LIMIT {int.Parse(AccessDatabaseStartPage.SelectedRowAmount, CultureInfo.CurrentCulture)} OFFSET {dataOffset}";
        }
        Dispatcher.InvokeAsync(LoadDataAsync);
        this.Loaded += ViewDatabaseTable_Loaded;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            string columns = string.Join(", ", columnSelectionList);
            SqliteCommand command = new($"SELECT {columns} FROM {AccessDatabaseStartPage.SelectedTable} {limitString}", connection);
            DataTable tempDataTable = new();
            await using (SqliteDataReader reader = await command.ExecuteReaderAsync())
            {
                tempDataTable.Load(reader);
            }
            Dispatcher.Invoke(() =>
            {
                dataTable.Merge(tempDataTable);
                DataRow bla;
                bla = dataTable.NewRow();
                bla["itemId"] = "123456";
                bla.SetParentRow(dataTable.Rows[0]);
                dataTable.Rows.Add(bla);
                Log.Information(bla.RowState.ToString());
                myDataGrid.ItemsSource = dataTable.AsDataView();

            }, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to load data.", ex);
            ExceptionHandling.ExceptionHandler("Catched in ViewTablePage->LoadDataAsync", ex);
        }
    }

    private async Task LoadSalesDetails()
    {
        try
        {
            string connectionString = $"{DatabaseMain.connString}";
            using SqliteConnection connection = new(connectionString);
            connection.Open();

            // Query to get summary by userId
            string summaryByUserIdQuery = @"
            SELECT otherPlayerId, SUM(quantity) AS totalQuantity, SUM(quantity * price) AS totalIncome
            FROM sales
            GROUP BY otherPlayerId";
            using SqliteCommand summaryByUserIdCmd = new(summaryByUserIdQuery, connection);
            using SqliteDataReader reader = summaryByUserIdCmd.ExecuteReader();
            while (reader.Read())
            {
                int userId = reader.GetInt32(0);
                int totalQuantity = reader.GetInt32(1);
                int totalIncome = reader.GetInt32(2);
                Console.WriteLine($"SUMMARY for userId: {userId}\tTotal Quantity sold: {totalQuantity}\tTotal Income: {totalIncome}");

                // Query to get summary by itemName for each userId
                string summaryByItemNameQuery = @"
                SELECT itemName, SUM(quantity) AS totalQuantity, SUM(quantity * price) AS totalIncome
                FROM sales
                WHERE otherPlayerId = @userId
                GROUP BY itemName";
                using SqliteCommand summaryByItemNameCmd = new(summaryByItemNameQuery, connection);
                summaryByItemNameCmd.Parameters.AddWithValue("@userId", userId);
                using SqliteDataReader itemReader = summaryByItemNameCmd.ExecuteReader();
                while (itemReader.Read())
                {
                    string itemName = itemReader.GetString(0);
                    int itemTotalQuantity = itemReader.GetInt32(1);
                    int itemTotalIncome = itemReader.GetInt32(2);
                    Console.WriteLine($"SUMMARY for {itemName}\tTotal Quantity sold: {itemTotalQuantity}\tTotal Income: {itemTotalIncome}");

                    // Query to get detailed sales for each itemName and userId
                    string detailedSalesQuery = @"
                    SELECT itemName, quantity, price, otherPlayerId
                    FROM sales
                    WHERE otherPlayerId = @userId AND itemName = @itemName";
                    using SqliteCommand detailedSalesCmd = new(detailedSalesQuery, connection);
                    detailedSalesCmd.Parameters.AddWithValue("@userId", userId);
                    detailedSalesCmd.Parameters.AddWithValue("@itemName", itemName);
                    using SqliteDataReader detailReader = detailedSalesCmd.ExecuteReader();
                    while (detailReader.Read())
                    {
                        string detailItemName = detailReader.GetString(0);
                        int quantity = detailReader.GetInt32(1);
                        int price = detailReader.GetInt32(2);
                        int detailUserId = detailReader.GetInt32(3);
                        Console.WriteLine($"{detailItemName}\t{quantity}\t{price}\t{detailUserId}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to load data.", ex);
            ExceptionHandling.ExceptionHandler("Catched in ViewTablePage->LoadDataAsync", ex);
        }
    }

    private async void LoadMoreData()
    {
        await LoadDataAsync();
        if (limitString != "")
        {
            limitString = $"LIMIT {int.Parse(AccessDatabaseStartPage.SelectedRowAmount, CultureInfo.CurrentCulture)} OFFSET {dataOffset += int.Parse(AccessDatabaseStartPage.SelectedRowAmount, CultureInfo.CurrentCulture)}";
        }
    }

    private void MyDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        Forms.MessageBox.Show("Right Clicked");
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        switch ((sender as MenuItem).Header)
        {
            case "Open":
                Forms.MessageBox.Show("Clicked open");
                break;

            case "Delete":
                Forms.MessageBox.Show("Delete");
                break;

            case "Add":
                Forms.MessageBox.Show("Add");
                break;

            default:
                break;
        }
    }

    private void ViewDatabaseTable_Loaded(object sender, RoutedEventArgs e)
    {
        ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(myDataGrid);
        if (scrollViewer != null)
        {
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        ScrollViewer scrollViewer = sender as ScrollViewer;
        if (scrollViewer == null)
        {
            return;
        }

        // Check if the scroll has reached the end
        if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
        {
            LoadMoreData();
        }
    }

    private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);
            if (child is T t)
            {
                return t;
            }
            else
            {
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
        }
        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        throw new NotImplementedException();
    }
}