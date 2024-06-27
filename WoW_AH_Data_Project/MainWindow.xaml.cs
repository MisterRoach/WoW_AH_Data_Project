using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;
using TSM_Data_Converter;
using System.IO;
using System.Reflection;

namespace WoW_AH_Data_Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        // Function/Method that's executed if user presses the button to open Lua Conversion Window
        private void Btn_choose_lua_conv_Click(object sender, RoutedEventArgs e)
        {
            // Make window object
            TSMLuaConvWindow tSMLuaConvWindow = new TSMLuaConvWindow();
            // Show window
            tSMLuaConvWindow.Show();
        }
    }
}