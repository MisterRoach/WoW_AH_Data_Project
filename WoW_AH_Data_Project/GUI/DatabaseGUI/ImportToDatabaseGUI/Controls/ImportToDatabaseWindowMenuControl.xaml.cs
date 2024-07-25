using System.Windows;
using WoWAHDataProject.GUI.DatabaseGUI.ImportToDatabaseGUI.ViewModels;

namespace WoWAHDataProject.GUI.DatabaseGUI.ImportToDatabaseGUI.Controls;

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
}
