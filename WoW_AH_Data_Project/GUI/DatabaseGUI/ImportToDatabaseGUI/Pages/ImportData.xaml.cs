using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Serilog;
using WoWAHDataProject.Code;
using WoWAHDataProject.Database;
using WinForms = System.Windows.Forms;

namespace WoWAHDataProject.GUI.DatabaseGUI.ImportToDatabaseGUI.Pages;

public partial class ImportData : Page, INotifyPropertyChanged
{
    private readonly List<string> files = [];
    public ImportData()
    {
        InitializeComponent();
        DataContext = this;
    }
    private void BtnBrowseSalesNPurchases_Click(object sender, RoutedEventArgs e)
    {
        files.Clear();
        BtnStartLuaFileImport.Visibility = Visibility.Collapsed;
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
                foreach (string file in Directory.EnumerateFiles(dialog.SelectedPath, "*purchases*.csv"))
                {
                    Log.Information($"Found file to import: {file}");
                    files.Add(file);
                }
                foreach (string file in Directory.EnumerateFiles(dialog.SelectedPath, "*sales*.csv"))
                {
                    Log.Information($"Found file to import: {file}");
                    files.Add(file);
                }
                ProgressionBar.Maximum = files.Count;
                ProgressionBar.Value = 0;
                ProgressionBar.Visibility = Visibility.Visible;
                BtnImportText = $"Start import of {files.Count} files";
                BtnStartSalesNPurchasesImport.IsEnabled = true;
                BtnStartSalesNPurchasesImport.Visibility = Visibility.Visible;
            }
        }
    }

    private async void BtnStartSalesNPurchasesImport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Run(() => DatabaseImportCsvs.DatabaseImportCsvFiles(files, $@"Data Source={AppDomain.CurrentDomain.BaseDirectory}db\maindatabase.db", this));
            WinForms.MessageBox.Show("Import completed!", "Import completed!", MessageBoxButtons.OK);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while importing csv files");
            ExceptionHandling.ExceptionHandler("Error after clicking StartSalesNPurchasesImport", ex);
        }
        finally
        {
            ProgressionTextBlock.Visibility = Visibility.Collapsed;
            ProgressionBar.Visibility = Visibility.Collapsed;
            BtnStartSalesNPurchasesImport.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnBrowseLuaFile_Click(object sender, RoutedEventArgs e)
    {
        files.Clear();
        BtnStartSalesNPurchasesImport.Visibility = Visibility.Collapsed;
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
                foreach (string file in Directory.EnumerateFiles(dialog.SelectedPath, "*AppData*.lua"))
                {
                    Log.Information($"Found file to import: {file}");
                    files.Add(file);
                }
                ProgressionBar.Maximum = files.Count;
                ProgressionBar.Value = 0;
                ProgressionBar.Visibility = Visibility.Visible;
                BtnImportText = $"Start import of {files.Count} files";
                BtnStartLuaFileImport.IsEnabled = true;
                BtnStartLuaFileImport.Visibility = Visibility.Visible;
            }
        }
    }

    public async void BtnStartLuaFileImportClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await DatabaseImportMarketValues.DatabaseImportLuaMarketValues(files, this);
            WinForms.MessageBox.Show("Import completed!", "Import completed!", MessageBoxButtons.OK);
        }
        catch (Exception ex)
        {
            Log.Error("Error while importing lua files:\n", ex);
        }
        finally
        {
            ProgressionTextBlock.Visibility = Visibility.Collapsed;
            ProgressionBar.Visibility = Visibility.Collapsed;
            BtnStartLuaFileImport.Visibility = Visibility.Collapsed;
        }
    }
    private string _BtnImportText;
    public string BtnImportText
    {
        get => _BtnImportText;
        set
        {
            _BtnImportText = value;
            OnPropertyChanged();
        }
    }
    private decimal _ProgressValue;
    public decimal ProgressValue
    {
        get => _ProgressValue;
        set
        {
            _ProgressValue = value;
            OnPropertyChanged();
        }
    }
    private string _ProgressionText;
    public string ProgressionText
    {
        get => _ProgressionText;
        set
        {
            _ProgressionText = value;
            OnPropertyChanged();
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler handler = this.PropertyChanged;
        if (handler != null)
        {
            PropertyChangedEventArgs e = new(propertyName);
            handler(this, e);
        }
    }
}
