﻿using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using Serilog;
using WoWAHDataProject.Database;
using WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.ViewModels;

namespace WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.Pages;
/// <summary>
/// Interaction logic for Start.xaml
/// </summary>
public partial class AccessDatabaseStartPage : Page
{
    private readonly ObservableCollection<ComponentTrackListitemsState> viewCollection = [];
    private static readonly SqliteConnection connection = new(DatabaseMain.connString);
    internal static string SelectedTable;
    internal static string SelectedRowAmount;
    internal static List<string> columnSelectionList = [];
    public AccessDatabaseStartPage()
    {
        InitializeComponent();
        ComboBoxAmountOfRowsToDisplay.Items.Add("All");
        ComboBoxAmountOfRowsToDisplay.Items.Add("1000");
        ComboBoxAmountOfRowsToDisplay.Items.Add("500");
        ComboBoxAmountOfRowsToDisplay.Items.Add("250");
        connection.Open();
    }

    private void BtnSelectTable_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Collections.IEnumerable content = ListViewTable.Items.SourceCollection;
            foreach (ComponentTrackListitemsState entry in content.Cast<ComponentTrackListitemsState>().ToList())
            {
                if (entry.IsChecked)
                {
                    columnSelectionList.Add(entry.ColumnName);
                }
            }
            AccessDatabaseWindowMenuItemsData.NavigateToPage("ViewTablePage");
        }
        catch (Exception ex)
        {
            Log.Error("Failed to open table view.", ex);
        }
    }
    private void DatabaseComboBoxDropDownOpened(object sender, EventArgs e)
    {
        SqliteCommand countCommand = new("SELECT COUNT(name) FROM sqlite_master WHERE type='table';", connection);
        SqliteCommand selectCommand = new("SELECT name FROM sqlite_master WHERE type='table';", connection);
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
        SelectedTable = DatabaseComboBox.SelectedItem.ToString();
        if (ListViewTable.Visibility == Visibility.Collapsed)
        {
            ListViewTable.Visibility = Visibility.Visible;
        }
    }
    private void GetTableColumns(string tableName)
    {
        viewCollection.Clear();
        SqliteCommand command = new($"SELECT name FROM pragma_table_info('{tableName}');", connection);
        SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Log.Information(reader.GetString(0));
            viewCollection.Add(new ComponentTrackListitemsState { IsChecked = true, ColumnName = reader.GetString(0) });
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

    private void ComboBoxAmountOfRowsToDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedRowAmount = ComboBoxAmountOfRowsToDisplay.SelectedItem.ToString();
    }

    private void ComboBoxAmountOfRowsToDisplay_DropDownOpened(object sender, EventArgs e)
    {

    }

    private void ComboBoxToGroupByColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void ComboBoxToGroupByColumn_DropDownOpened(object sender, EventArgs e)
    {
        foreach (ComponentTrackListitemsState item in viewCollection)
        {
            if (item.IsChecked)
            {
                ComboBoxToGroupByColumn.Items.Add(item.ColumnName);
            }
        }
        ComboBoxToGroupByColumn.Items.Refresh();
    }

    private void ComboBoxToGroupByFurther_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void ComboBoxToGroupByFurther_DropDownOpened(object sender, EventArgs e)
    {
        foreach (ComponentTrackListitemsState item in viewCollection)
        {
            if (item.IsChecked)
            {
                ComboBoxToGroupByFurther.Items.Add(item.ColumnName);
            }
        }
        ComboBoxToGroupByFurther.Items.Refresh();
    }

    private void ComboBoxToOrderBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void ComboBoxToOrderBy_DropDownOpened(object sender, EventArgs e)
    {
        foreach (ComponentTrackListitemsState item in viewCollection)
        {
            if (item.IsChecked)
            {
                ComboBoxToOrderBy.Items.Add(item.ColumnName);
            }
        }
        ComboBoxToOrderBy.Items.Refresh();
    }

    private void ComboBoxToOrderByFurther_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void ComboBoxToOrderByFurther_DropDownOpened(object sender, EventArgs e)
    {
        foreach (ComponentTrackListitemsState item in viewCollection)
        {
            if (item.IsChecked)
            {
                ComboBoxToOrderByFurther.Items.Add(item.ColumnName);
            }
        }
        ComboBoxToOrderByFurther.Items.Refresh();
    }
}
