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
        // Variables for the AppHelpers AppData.lua path and the output path where the created csvs are going
        string lua_file_path = "";
        string csv_output_path = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        // Function/Method that's executed if the button to select TSM AppHelper path is pressed
        private void btn_sel_tsm_path_Click(object sender, RoutedEventArgs e)
        {
            // Dialog variable to browse path
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            // Default path to browse from is the path of this file's execution
            dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Pop Dialog and make variable to store the button pressed by user when selecting the path
            WinForms.DialogResult result = dialog.ShowDialog();
            // If user pressed "ok" check if the AppData.lua file exists in the chosen path
            if (result == WinForms.DialogResult.OK)
            {
                if (!File.Exists(dialog.SelectedPath + "\\AppData.lua"))
                {
                    // If the file AppData.lua file doesn't exists in the chosen path display an error message
                    DialogResult err_result;
                    err_result = WinForms.MessageBox.Show("Could not find AppData.lua", "Error", MessageBoxButtons.OK);
                    if (err_result == WinForms.DialogResult.OK)
                    {
                        // close error message when "ok" is pressed
                        return;
                    }
                }
                // Else, if AppData.lua file exists in the chosen path, set the lua_file_path variable to the selected path and add the actual filename to it
                else
                {
                    // Set text for the tsm path selection textbox to the selected path
                    txtb_sel_tsm_path.Text = dialog.SelectedPath;
                    // Set the lua_file_path variable
                    lua_file_path = dialog.SelectedPath + "\\AppData.lua";
                    // Also enable the button that's used to start conversion
                    btn_conv_lua_data.IsEnabled = true;
                }
            }
        }

        // Function/Method that's executed if the button to start conversion is pressed
        private void btn_conv_lua_data_Click(object sender, RoutedEventArgs e)
        {
            // If no path for the csv outputs was selected, set it to the path of this file's execution
            if(txtb_sel_output_path.Text == "")
            {
                csv_output_path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            // Call method to convert the data
            TSM_Data_Converter.TSM_Lua_To_csv.TSM_Lua_AH_Values_Converter(lua_file_path, csv_output_path);
        }

        // Function/Method that's executed if the button to select csv output path is pressed
        private void btn_sel_output_path_Click(object sender, RoutedEventArgs e)
        {
            // Dialog variable to browse path
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            // Default path to browse from is the path of this file's execution
            dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Pop Dialog and make variable to store the button pressed by user when selecting the path
            WinForms.DialogResult result = dialog.ShowDialog();
            // If user pressed "ok" set the variable for the csv output path to the selected path
            if (result == WinForms.DialogResult.OK)
            {
                txtb_sel_output_path.Text = dialog.SelectedPath;
                csv_output_path = dialog.SelectedPath;
            }
        }
    }
}