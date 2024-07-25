using System.Windows;
using System.Windows.Media;
using Serilog;
using WoWAHDataProject.GUI.MainWindowGUI.ViewModels;

namespace WoWAHDataProject.GUI.MainWindowGUI.Controls;

public partial class MainWindowMenuControl : System.Windows.Controls.UserControl
{
    public MainWindowMenuControl()
    {
        InitializeComponent();
        DataContext = new MainWindowMenuViewModel();
    }

    public Thickness SubMenuPadding
    {
        get => (Thickness)GetValue(SubMenuPaddingProperty);
        set => SetValue(SubMenuPaddingProperty, value);
    }

    public static readonly DependencyProperty SubMenuPaddingProperty =
        DependencyProperty.Register("SubMenuPadding", typeof(Thickness), typeof(MainWindowMenuControl));

    public void MainMenuClick(object sender, RoutedEventArgs e)
    {
        // If mainmenu database button clicked, iterate through tree to uncheck submenu items if they checked
        if ((sender as System.Windows.Controls.RadioButton).Content.ToString() == "Database")
        {
            Log.Information("Navigating to Database Page");
            System.Windows.Window window = System.Windows.Application.Current.MainWindow;
            IterateThroughTree((window as MainWindow).rootGrid);
        }
    }

    public static void IterateThroughTree(DependencyObject dependencyObject)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
            Log.Information("Further Iterating through Object: " + dependencyObject);
            Log.Information("IteratingFurther Child: " + child.ToString());
            if (child is System.Windows.Controls.RadioButton)
            {
                if ((child as System.Windows.Controls.RadioButton).GroupName == "SubMenu")
                {
                    (child as System.Windows.Controls.RadioButton).IsChecked = false;
                }
                Log.Information("Found RadioButton with Group: " + (child as System.Windows.Controls.RadioButton).GroupName);
            }
            IterateThroughTree(child);
        }
    }
}