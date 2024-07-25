using System.IO;
using System.Reflection;
using System.Windows;
using Serilog;
using WoWAHDataProject.Code;
using WinForms = System.Windows.Forms;

namespace WoWAHDataProject.GUI.DatabaseGUI;
public partial class WindowImportCsvsToDatabase : Window
{
    List<string> files = new();
    public WindowImportCsvsToDatabase()
    {
        InitializeComponent();
        if (Egg.音)
        {
            Egg.Hatch芸術プロ生ちゃんEgg(this);
        }
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
                TxtbSelectCsvsPath.Text = dialog.SelectedPath;
                BtnImportCsvsToDb.IsEnabled = true;
            }
        }
    }

    private async void BtnImportCsvsToDbClick(object sender, RoutedEventArgs e)
    {
        //await Task.Run(() => DatabaseImportCsvs.DatabaseImportCsvFiles(files, $@"Data Source={AppDomain.CurrentDomain.BaseDirectory}db\maindatabase.db"));
    }
}
