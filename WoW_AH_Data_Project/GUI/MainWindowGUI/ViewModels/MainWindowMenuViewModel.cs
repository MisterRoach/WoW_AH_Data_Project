using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Serilog;
using Application = System.Windows.Application;

namespace WoWAHDataProject.GUI.MainWindowGUI.ViewModels;

internal class MainWindowMenuViewModel
{
    //Our Source List for Menu Items
    public static List<MainWindowMenuItemsData> MenuList =>
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
        // Look for MainWindow, then navigate to menu page
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

    private void Execute()
    {
        //our logic comes here
        string SubMenuItem = SubMenuText.Replace(" ", string.Empty).ToLowerInvariant();
        try
        {
            if (!string.IsNullOrEmpty(SubMenuItem))
            {
                NavigateToPage(SubMenuItem);
            }
        }
        catch (Exception ex)
        {
            Log.Information("Awww it failed", ex);
        }
    }

    public static void NavigateToPage(string Menu)
    {
        // Look for MainWindow, then navigate to sub menu page
        foreach (Window window in Application.Current.Windows)
        {
            if (window.GetType() == typeof(MainWindow))
            {
                (window as MainWindow).MainWindowFrame.Navigate(new Uri(string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", "GUI/MainWindowGUI/Pages/", Menu, ".xaml"), UriKind.RelativeOrAbsolute));
            }
        }
    }
}