using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Data.Sqlite;
using Serilog;
using WoWAHDataProject.Database;
using WoWAHDataProject.GUI.Db;
using Application = System.Windows.Application;

namespace WoWAHDataProject.GUI.MainWindowGUI.ViewModels;

internal class MainWindowMenuViewModel
{
    //Our Source List for Menu Items
    public List<MainWindowMenuItemsData> MenuList =>
    [
                //MainMenu without SubMenu Button 
                new MainWindowMenuItemsData(){ MenuText="Start", SubMenuList=null},

                //MainMenu Button
                new MainWindowMenuItemsData(){ MenuText="Database"

                //SubMenu Button
                , SubMenuList=[
                new MainWindowSubMenuItemsData(){ SubMenuText="Create Database" },
                new MainWindowSubMenuItemsData(){ SubMenuText="Access Database" },
                ]},

                //MainMenu without SubMenu Button
                new MainWindowMenuItemsData(){ MenuText="Settings", SubMenuList=null}
            ];
}

public class MainWindowMenuItemsData
{
    public string MenuText { get; set; }
    public List<MainWindowSubMenuItemsData> SubMenuList { get; set; }

    //To Add click event to our Buttons we will use ICommand here to switch pages when specific button is clicked
    public MainWindowMenuItemsData()
    {
        Command = new MainWindowCommandViewModel(Execute);
    }

    public ICommand Command { get; }

    private void Execute()
    {
        //our logic comes here
        string MT = MenuText.Replace(" ", string.Empty);
        try
        {
            if (!string.IsNullOrEmpty(MT))
            {
                NavigateToPage(MT);
            }
        }
        catch (Exception ex)
        {
            Log.Information("Awww it failed", ex);
        }

    }

    private static void NavigateToPage(string Menu)
    {
        //We will search for our Main Window in open windows and then will access the frame inside it to set the navigation to desired page..
        //lets see how... ;)
        foreach (Window window in Application.Current.Windows)
        {
            if (window.GetType() == typeof(MainWindow))
            {
                (window as MainWindow).MainWindowFrame.Navigate(new Uri(string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", "GUI/MainWindowGUI/Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
            }
        }
    }
}
public class MainWindowSubMenuItemsData
{
    public string SubMenuText { get; set; }

    //To Add click event to our Buttons we will use ICommand here to switch pages when specific button is clicked
    public MainWindowSubMenuItemsData()
    {
        SubMenuCommand = new MainWindowCommandViewModel(Execute);
    }

    public ICommand SubMenuCommand { get; }

    private async void Execute()
    {
        //our logic comes here
        string SMT = SubMenuText.Replace(" ", string.Empty).ToLowerInvariant();
        Log.Information("SubMenuText: " + SMT);
        if (SMT.Contains("accessdatabase"))
        {
            Log.Information("In smt: if adb");
            MW window = new();
            window.Show();
            //Window windowAccessDatabase = await WindowAccessDatabase.CreateAsync(new SqliteConnection(DatabaseMain.connString));
            //windowAccessDatabase.Show();
        }
        else if (!string.IsNullOrEmpty(SMT))
        {
            Log.Information("In smt: else if !");
            NavigateToPage(SMT);
        }
    }

    private static void NavigateToPage(string Menu)
    {
        //We will search for our Main Window in open windows and then will access the frame inside it to set the navigation to desired page..
        //lets see how... ;)
        foreach (Window window in Application.Current.Windows)
        {
            if (window.GetType() == typeof(MainWindow))
            {
                (window as MainWindow).MainWindowFrame.Navigate(new Uri(string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", "GUI/MainWindowGUI/Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
            }
        }
    }
}
