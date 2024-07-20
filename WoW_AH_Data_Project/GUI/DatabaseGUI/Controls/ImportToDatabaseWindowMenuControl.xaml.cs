using System.Windows;
using WoWAHDataProject.GUI.DatabaseGUI.ViewModels;

namespace WoWAHDataProject.GUI.DatabaseGUI.Controls;

public partial class ImportToDatabaseWindowMenuControl : System.Windows.Controls.UserControl
{
    public ImportToDatabaseWindowMenuControl()
    {
        InitializeComponent();

        //Binding our ViewModel with the datacontext to read the Menu & SubMenuItemsData
        DataContext = new ImportToDatabaseWindowMenuViewModel();
    }


    public Thickness SubMenuPadding
    {
        get => (Thickness)GetValue(SubMenuPaddingProperty);
        set => SetValue(SubMenuPaddingProperty, value);
    }

    // Using a DependencyProperty as the backing store for SubMenuPadding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SubMenuPaddingProperty =
        DependencyProperty.Register("SubMenuPadding", typeof(Thickness), typeof(ImportToDatabaseWindowMenuControl));



    public bool HasIcon
    {
        get => (bool)GetValue(HasIconProperty);
        set => SetValue(HasIconProperty, value);
    }

    // Using a DependencyProperty as the backing store for HasIcon.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HasIconProperty =
        DependencyProperty.Register("HasIcon", typeof(bool), typeof(ImportToDatabaseWindowMenuControl));
}
