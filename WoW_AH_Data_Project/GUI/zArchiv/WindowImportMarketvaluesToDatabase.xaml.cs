﻿using System.IO;
using System.Reflection;
using System.Windows;
using Serilog;
using WoWAHDataProject.Code;
using WinForms = System.Windows.Forms;
namespace WoWAHDataProject.GUI.DatabaseGUI;
/// <summary>
/// Window for Lua File Data Import to Database
/// </summary>
public partial class WindowImportMarketvaluesToDatabase : Window
{
    List<string> files = [];
    public WindowImportMarketvaluesToDatabase()
    {
        InitializeComponent();
        if (Egg.音)
        {
            Egg.Hatch芸術プロ生ちゃんEgg(this);
        }
    }

    private void BtnSelectTsmAppHelperPathClick(object sender, RoutedEventArgs e)
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
                TxtbSelectTsmAppHelperPath.Text = dialog.SelectedPath;
                BtnStartMarketValuesImportToDb.IsEnabled = true;
            }
        }
    }

    private async void BtnStartMarketValuesImportToDbClick(object sender, RoutedEventArgs e)
    {
        //await Task.Run(() => DatabaseImportMarketValues.DatabaseImportLuaMarketValues(files, $@"Data Source={AppDomain.CurrentDomain.BaseDirectory}db\maindatabase.db"));
    }
}
