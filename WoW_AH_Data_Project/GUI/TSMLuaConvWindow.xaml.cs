namespace WoWAHDataProject.GUI;
using System.IO;
using System.Reflection;
using System.Windows;
using WoWAHDataProject.Code;
using WinForms = System.Windows.Forms;
/// <summary>
/// Window for Lua File Data Conversion
/// </summary>
public partial class TSMLuaConvWindow : Window
{
    string luaFilePath = "";
    string csvOutputFilePath = "";

    public TSMLuaConvWindow()
    {
        InitializeComponent();
        if (Egg.音)
        {
            Egg.Hatch芸術プロ生ちゃんEgg(this);
        }
    }

    private void BtnSelectTsmPathClick(object sender, RoutedEventArgs e)
    {
        WinForms.FolderBrowserDialog dialog = new()
        {
            InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        };

        WinForms.DialogResult result = dialog.ShowDialog();
        if (result == WinForms.DialogResult.OK)
        {
            if (!File.Exists(dialog.SelectedPath + "\\AppData.lua"))
            {
                DialogResult errResult = WinForms.MessageBox.Show("Could not find AppData.lua", "Error", MessageBoxButtons.OK);
                if (errResult == WinForms.DialogResult.OK)
                {
                    return;
                }
            }
            else
            {
                TxtbSelectTsmPath.Text = dialog.SelectedPath;
                luaFilePath = dialog.SelectedPath + "\\AppData.lua";
                BtnConvertLuaData.IsEnabled = true;
            }
        }
    }

    private void BtnConvertLuaDataClick(object sender, RoutedEventArgs e)
    {
        if (TxtbSelectOutputPath.Text?.Length == 0)
        {
            csvOutputFilePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        WoWAHDataProject.Code.TSMLuaConverter.TSMLuaAHValuesConverter(luaFilePath, csvOutputFilePath);
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
            csvOutputFilePath = dialog.SelectedPath;
        }
    }
}
