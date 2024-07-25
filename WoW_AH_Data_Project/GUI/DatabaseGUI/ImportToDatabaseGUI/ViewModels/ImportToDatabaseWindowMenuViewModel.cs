using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Serilog;
using Application = System.Windows.Application;

namespace WoWAHDataProject.GUI.DatabaseGUI.ImportToDatabaseGUI.ViewModels;

internal class ImportToDatabaseWindowMenuViewModel
{
    //Our Source List for Menu Items
    public static List<ImportToDatabaseWindowMenuItemsData> MenuList =>
    [
                new ImportToDatabaseWindowMenuItemsData(){ MenuText="Start", SubMenuList=null},

                new ImportToDatabaseWindowMenuItemsData(){ MenuText="Import Data", SubMenuList=null},

                new ImportToDatabaseWindowMenuItemsData(){ MenuText="Settings", SubMenuList=null}
            ];
}

public class ImportToDatabaseWindowMenuItemsData
{
    public string MenuText { get; set; }
    public List<ImportToDatabaseWindowSubMenuItemsData> SubMenuList { get; set; }

    //To Add click event to our Buttons we will use ICommand here to switch pages when specific button is clicked
    public ImportToDatabaseWindowMenuItemsData()
    {
        Command = new ImportToDatabaseWindowCommandViewModel(Execute);
    }

    public ICommand Command { get; }

    private void Execute()
    {
        //our logic comes here
        string MainMenuItem = MenuText.Replace(" ", string.Empty);
        try
        {
            if (!string.IsNullOrEmpty(MainMenuItem))
            {
                NavigateToPage(MainMenuItem);
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
            if (window.GetType() == typeof(ImportToDatabaseWindow))
            {
                (window as ImportToDatabaseWindow).MainWindowFrame.Navigate(new Uri(string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", "GUI/DatabaseGUI/ImportToDatabaseGUI/Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
            }
        }
    }
}
public class ImportToDatabaseWindowSubMenuItemsData
{
    public string SubMenuText { get; set; }

    //To Add click event to our Buttons we will use ICommand here to switch pages when specific button is clicked
    public ImportToDatabaseWindowSubMenuItemsData()
    {
        SubMenuCommand = new ImportToDatabaseWindowCommandViewModel(Execute);
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
            if (window.GetType() == typeof(ImportToDatabaseWindow))
            {
                (window as ImportToDatabaseWindow).MainWindowFrame.Navigate(new Uri(string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", "GUI/DatabaseGUI/ImportToDatabaseGUI/Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
            }
        }
    }
}
