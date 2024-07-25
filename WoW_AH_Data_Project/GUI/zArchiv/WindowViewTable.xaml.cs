using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;
using Forms = System.Windows.Forms;

namespace WoWAHDataProject.GUI.DatabaseGUI;

/// <summary>
/// Interaction logic for DbTestWindow.xaml
/// </summary>
public partial class WindowViewTable : Window, IDisposable
{
    private static SqliteConnection connection;
    private int dataOffset;
    public const int dataLimit = 100; // Adjust based on your needs
    private readonly string tableName;
    private readonly List<string> columnSelectionList;
    private readonly DataTable dataTable;

    public WindowViewTable(string tableName, List<string> columnSelectionList, SqliteConnection sqlConnection)
    {
        InitializeComponent();
        this.Loaded += ViewDatabaseTable_Loaded;
        this.columnSelectionList = columnSelectionList;
        this.tableName = tableName;
        this.dataTable = new DataTable(tableName);
        connection = sqlConnection;
    }
    public async Task LoadDataAsync()
    {
        string columns = string.Join(", ", columnSelectionList);
        SqliteCommand command = new($"SELECT {columns} FROM {tableName} LIMIT {dataLimit} OFFSET {dataOffset}", connection);
        DataTable tempDataTable = new();
        await using (SqliteDataReader reader = await command.ExecuteReaderAsync())
        {
            tempDataTable.Load(reader);
        }
        Dispatcher.Invoke(() =>
        {
            dataTable.Merge(tempDataTable);
            myDataGrid.ItemsSource = dataTable.AsDataView();
        }, DispatcherPriority.Background);
    }
    private async void LoadMoreData()
    {
        await LoadDataAsync();
        dataOffset += dataLimit; // Prepare for the next load
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
