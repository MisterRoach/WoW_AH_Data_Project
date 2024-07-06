namespace WoWAHDataProject;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using WinForms = System.Windows.Forms;

public partial class CombineCSVsWindow : Window
{
    string csvOutputPath = "";
    string[] purchasesCsvPath;
    string[] salesCsvPath;
    public CombineCSVsWindow()
    {
        InitializeComponent();
    }

    private void BtnSelectCsvsPathClick(object sender, RoutedEventArgs e)
    {
        WinForms.FolderBrowserDialog dialog = new()
        {
            InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        };

        WinForms.DialogResult result = dialog.ShowDialog();
        if (result == WinForms.DialogResult.OK)
        {
            bool purchasesCsvExist = Directory.EnumerateFiles(dialog.SelectedPath, "*purchases.csv").Any();
            bool salesCsvExist = Directory.EnumerateFiles(dialog.SelectedPath, "*sales.csv").Any();
            // Check if files exist
            if (!purchasesCsvExist || !salesCsvExist)
            {
                DialogResult errResult = WinForms.MessageBox.Show("Could not find one or both csv files", "Error", MessageBoxButtons.OK);
                if (errResult == WinForms.DialogResult.OK)
                {
                    return;
                }
            }
            else
            {
                purchasesCsvPath = Directory.GetFiles(dialog.SelectedPath, "*purchases*.csv");
                salesCsvPath = Directory.GetFiles(dialog.SelectedPath, "*sales*.csv");
                TxtbSelectCsvsPath.Text = dialog.SelectedPath;
                // Enable button that's used to start conversion
                BtnCombineCsvs.IsEnabled = true;
            }
        }
    }

    private void BtnSelectOutputPathClick(object sender, RoutedEventArgs e)
    {
        WinForms.FolderBrowserDialog dialog = new()
        {
            InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        };

        WinForms.DialogResult result = dialog.ShowDialog();
        if (result == WinForms.DialogResult.OK)
        {
            TxtbSelectOutputPath.Text = dialog.SelectedPath;
            csvOutputPath = dialog.SelectedPath;
        }
    }

    private void BtnCombineCsvsClick(object sender, RoutedEventArgs e)
    {
        if (TxtbSelectOutputPath.Text?.Length == 0)
        {
            csvOutputPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        WoWAHDataProject.Code.CombineSalesPurchasesCsvs.CombineSalesPurchasesCsv(purchasesCsvPath[0], salesCsvPath[0], csvOutputPath);
    }
}
