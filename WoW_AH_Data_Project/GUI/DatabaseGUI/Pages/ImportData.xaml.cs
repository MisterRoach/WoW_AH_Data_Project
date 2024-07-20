using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Serilog;
using WoWAHDataProject.Code;
using WoWAHDataProject.Database;
using WinForms = System.Windows.Forms;

namespace WoWAHDataProject.GUI.DatabaseGUI.Pages;
/// <summary>
/// Interaction logic for Import_Data.xaml
/// </summary>
public partial class ImportData : Page
{
    List<string> files = new();
    public ImportData()
    {
        InitializeComponent();
    }

    private void BtnBrowseSalesNPurchases_Click(object sender, RoutedEventArgs e)
    {
        WinForms.FolderBrowserDialog dialog = new()
        {
            InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        };

        WinForms.DialogResult result = dialog.ShowDialog();
        if (result == WinForms.DialogResult.OK)
        {
            bool purchasesCsvExist = Directory.EnumerateFiles(dialog.SelectedPath, "*purchases*.csv").Any();
            bool salesCsvExist = Directory.EnumerateFiles(dialog.SelectedPath, "*sales*.csv").Any();
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
                foreach (var file in Directory.EnumerateFiles(dialog.SelectedPath, "*purchases*.csv"))
                {
                    Log.Information($"Found file to import: {file}");
                    files.Add(file);
                }
                foreach (var file in Directory.EnumerateFiles(dialog.SelectedPath, "*sales*.csv"))
                {
                    Log.Information($"Found file to import: {file}");
                    files.Add(file);
                }
                //TxtbSelectCsvsPath.Text = dialog.SelectedPath;
                BtnStartSalesNPurchasesImport.Content = $"Start import of {files.Count} files";
                BtnStartSalesNPurchasesImport.IsEnabled = true;
                BtnStartSalesNPurchasesImport.Visibility = Visibility.Visible;
            }
        }
    }

    private async void BtnStartSalesNPurchasesImport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Run(() => DatabaseImportCsvs.DatabaseImportCsvFiles(files, $@"Data Source={AppDomain.CurrentDomain.BaseDirectory}db\maindatabase.db"));
            WinForms.MessageBox.Show("Import of your files completed!", "Import completed", MessageBoxButtons.OK);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while importing csv files");
            ExceptionHandling.ExceptionHandler("Error after clicking StartSalesNPurchasesImport", ex);
        }
        finally
        {
            files.Clear();
        }
    }

    private void BtnBrowseLuaFile_Click(object sender, RoutedEventArgs e)
    {
        WinForms.FolderBrowserDialog dialog = new()
        {
            InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        };

        WinForms.DialogResult result = dialog.ShowDialog();
        if (result == WinForms.DialogResult.OK)
        {
            if (!Directory.EnumerateFiles(dialog.SelectedPath, "*AppData*.lua").Any())
            {
                DialogResult errResult = WinForms.MessageBox.Show("Could not find AppData.lua", "Error", MessageBoxButtons.OK);
                if (errResult == WinForms.DialogResult.OK)
                {
                    return;
                }
            }
            else
            {
                foreach (var file in Directory.EnumerateFiles(dialog.SelectedPath, "*AppData*.lua"))
                {
                    Log.Information($"Found file to import: {file}");
                    files.Add(file);
                }
                BtnStartLuaFileImport.Content = $"Start import of {files.Count} files";
                BtnStartLuaFileImport.IsEnabled = true;
                BtnStartLuaFileImport.Visibility = Visibility.Visible;
            }
        }
    }

    private async void BtnStartLuaFileImport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await DatabaseImportMarketValues.DatabaseImportLuaMarketValues(files, $@"Data Source={AppDomain.CurrentDomain.BaseDirectory}db\maindatabase.db");
            WinForms.MessageBox.Show("Import of Lua files completed!", "Import completed", MessageBoxButtons.OK);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while importing lua files");
            ExceptionHandling.ExceptionHandler("Error after clicking StartLuaFileImport", ex);
        }
        finally
        {
            files.Clear();
        }
    }
}
