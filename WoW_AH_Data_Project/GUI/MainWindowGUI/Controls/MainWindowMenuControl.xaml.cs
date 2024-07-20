using System.Windows;
using WoWAHDataProject.GUI.MainWindowGUI.ViewModels;

namespace WoWAHDataProject.GUI.MainWindowGUI.Controls;

public partial class MainWindowMenuControl : System.Windows.Controls.UserControl
{
    public MainWindowMenuControl()
    {
        InitializeComponent();

        //Binding our ViewModel with the datacontext to read the Menu & SubMenuItemsData
        DataContext = new MainWindowMenuViewModel();
    }


    public Thickness SubMenuPadding
    {
        get => (Thickness)GetValue(SubMenuPaddingProperty);
        set => SetValue(SubMenuPaddingProperty, value);
    }

    // Using a DependencyProperty as the backing store for SubMenuPadding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SubMenuPaddingProperty =
        DependencyProperty.Register("SubMenuPadding", typeof(Thickness), typeof(MainWindowMenuControl));



    public bool HasIcon
    {
        get => (bool)GetValue(HasIconProperty);
        set => SetValue(HasIconProperty, value);
    }

    // Using a DependencyProperty as the backing store for HasIcon.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HasIconProperty =
        DependencyProperty.Register("HasIcon", typeof(bool), typeof(MainWindowMenuControl));
}
