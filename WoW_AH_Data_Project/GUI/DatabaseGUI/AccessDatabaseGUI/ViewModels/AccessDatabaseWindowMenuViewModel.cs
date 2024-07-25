using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using Serilog;
using Application = System.Windows.Application;

namespace WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.ViewModels;

internal class AccessDatabaseWindowMenuViewModel
{

}

public class AccessDatabaseWindowMenuItemsData
{
    public static void NavigateToPage(string Menu)
    {
        //We will search for our Main Window in open windows and then will access the frame inside it to set the navigation to desired page..
        //lets see how... ;)
        foreach (Window window in Application.Current.Windows)
        {
            if (window.GetType() == typeof(AccessDatabaseWindow))
            {
                (window as AccessDatabaseWindow).AccessDatabaseWindowFrame.Navigate(new Uri(string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", "GUI/DatabaseGUI/AccessDatabaseGUI/Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
            }
        }
    }
}
public class AccessDatabaseWindowSubMenuItemsData
{
    public string SubMenuText { get; set; }

    //To Add click event to our Buttons we will use ICommand here to switch pages when specific button is clicked
    public AccessDatabaseWindowSubMenuItemsData()
    {
        SubMenuCommand = new AccessDatabaseWindowCommandViewModel(Execute);
    }

    public ICommand SubMenuCommand { get; }

    private async void Execute()
    {
        //our logic comes here
        string SubMenuItem = SubMenuText.Replace(" ", string.Empty).ToLowerInvariant();

        if (SubMenuItem.Contains("createdatabase") || SubMenuItem.Contains("importtodatabase"))
        {
        }
        else if (!string.IsNullOrEmpty(SubMenuItem))
        {
            Log.Information("In smt: else if !");
            NavigateToPage(SubMenuItem);
        }
    }

    private static void NavigateToPage(string Menu)
    {
        //We will search for our Main Window in open windows and then will access the frame inside it to set the navigation to desired page..
        //lets see how... ;)
        foreach (Window window in Application.Current.Windows)
        {
            if (window.GetType() == typeof(AccessDatabaseWindow))
            {
                //(window as AccessDatabaseWindow).MainWindowFrame.Navigate(new Uri(string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", "GUI/DatabaseGUI/AccessDatabaseGUI/Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
            }
        }
    }
}
