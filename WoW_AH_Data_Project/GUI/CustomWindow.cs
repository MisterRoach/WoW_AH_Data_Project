using System.Windows;
namespace WoWAHDataProject.GUI;
public class CustomWindow : Window
{
    static CustomWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow),
        new FrameworkPropertyMetadata(typeof(CustomWindow)));
    }
}
