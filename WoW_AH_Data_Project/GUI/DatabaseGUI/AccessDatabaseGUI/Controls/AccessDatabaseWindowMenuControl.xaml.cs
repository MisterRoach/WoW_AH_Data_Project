using System.Windows;
using WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.ViewModels;

namespace WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.Controls;

public partial class AccessDatabaseWindowMenuControl : System.Windows.Controls.UserControl
{
    public AccessDatabaseWindowMenuControl()
    {
        InitializeComponent();

        //Binding our ViewModel with the datacontext to read the Menu & SubMenuItemsData
        DataContext = new AccessDatabaseWindowMenuViewModel();
    }


    public Thickness SubMenuPadding
    {
        get => (Thickness)GetValue(SubMenuPaddingProperty);
        set => SetValue(SubMenuPaddingProperty, value);
    }

    // Using a DependencyProperty as the backing store for SubMenuPadding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SubMenuPaddingProperty =
        DependencyProperty.Register("SubMenuPadding", typeof(Thickness), typeof(AccessDatabaseWindowMenuControl));
}
