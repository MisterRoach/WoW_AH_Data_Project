namespace WoW_AH_Data_Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;
using System.IO;
using WoW_AH_Data_Project.Code;

public partial class CombineCSVsWindow : Window
{
    // Variables for the csvs path and the output path where the combined csv is going
    string csvs_path = "";
    string csv_output_path = "";
    string[] purchases_csv_path;
    string[] sales_csv_path;
    public CombineCSVsWindow()
    {
        InitializeComponent();
    }

    // Function/Method that's executed if the button to select sales and puchases csv path is pressed
    private void Btn_sel_csvs_path_Click(object sender, RoutedEventArgs e)
    {
        // Dialog variable to browse path
        WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
        // Default path to browse from is the path of this file's execution
        dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // Pop Dialog and make variable to store the button pressed by user when selecting the path
        WinForms.DialogResult result = dialog.ShowDialog();
        // If user pressed "ok" check if the the files exists in the chosen path
        if (result == WinForms.DialogResult.OK)
        {
            bool purchases_csv_exist = Directory.EnumerateFiles(dialog.SelectedPath, "*purchases.csv").Any();
            bool sales_csv_exist = Directory.EnumerateFiles(dialog.SelectedPath, "*sales.csv").Any();
            if (!purchases_csv_exist || !sales_csv_exist)
            {
                // If one of the csv files doesn't exist in the chosen path display an error message
                DialogResult err_result;
                err_result = WinForms.MessageBox.Show("Could not find one or both csv files", "Error", MessageBoxButtons.OK);
                if (err_result == WinForms.DialogResult.OK)
                {
                    // Close error message when "ok" is pressed
                    return;
                }
            }
            // Else, if the files exist in the chosen path, set variables for the paths
            else
            {
                purchases_csv_path = Directory.GetFiles(dialog.SelectedPath, "*purchases.csv");
                sales_csv_path = Directory.GetFiles(dialog.SelectedPath, "*sales.csv");
                // Set text for the csvs path selection textbox to the selected path
                Txtb_sel_csvs_path.Text = dialog.SelectedPath;
                // Set the csvs_path variable
                csvs_path = dialog.SelectedPath;
                // Also enable the button that's used to start conversion
                Btn_combine_csvs.IsEnabled = true;
            }
        }
    }
    // Function/Method that's executed if the button to select csv output path is pressed
    private void Btn_sel_output_path_Click(object sender, RoutedEventArgs e)
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
            Txtb_sel_output_path.Text = dialog.SelectedPath;
            csv_output_path = dialog.SelectedPath;
        }
    }

    private void Btn_combine_csvs_Click(object sender, RoutedEventArgs e)
    {
        // If no path for the csv outputs was selected, set it to the path of this file's execution
        if (Txtb_sel_output_path.Text == "")
        {
            csv_output_path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        WoW_AH_Data_Project.Code.Combine_Sales_Purchases_Csv.Combine_Sales_Purchases_Csvs(purchases_csv_path[0], sales_csv_path[0], csv_output_path);
    }
}
