namespace WoW_AH_Data_Project;
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
using System.IO;
using System.Reflection;
using WoW_AH_Data_Project.Code;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        // Log current timestamp of the system to file when MainWindow is opened
        Functions.Log("Timestamp for current log session: " + DateTime.Now.ToString());
        // Log path from where the app is executed
        Functions.Log("Executed in path: " + System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        InitializeComponent();
    }
    // Function/Method that's executed if user presses the button to open window to convert the data of TSM AppHelper AppData.lua file
    private void Btn_choose_lua_conv_Click(object sender, RoutedEventArgs e)
    {
        // Make window object for window used to let user choose the path of the AppData.lua file and the output path for the converted data files
        TSMLuaConvWindow tSMLuaConvWindow = new TSMLuaConvWindow();
        // Show window
        tSMLuaConvWindow.Show();
    }
    // Function/Method that's executed if user presses the button to open CSV combination window
    private void Btn_choose_combine_csvs_Click(object sender, RoutedEventArgs e)
    {
        // Make window object for the window used to let user choose the path of the purchases and sales csv files and the output path for the profit csv
        CombineCSVsWindow combineCSVsWindow = new CombineCSVsWindow();
        // Show window
        combineCSVsWindow.Show();

    }
}