using Serilog;
using System.Globalization;
using System.Windows;
using WoWAHDataProject.Code;
using WoWAHDataProject.Database;
namespace WoWAHDataProject;
/// <summary>
/// Window the application opens with
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
            .WriteTo.File("logs/db_log_.txt",formatProvider: CultureInfo.CurrentCulture,rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Information($"Timestamp for current log session: {DateTime.Now}");

        // Check .config file for values and set them if they are not set in case of first launch or update them if needed
        ConfigurationCheck.InitConfigCheck();

        InitializeComponent();
    }
    private void BtnSelectLuaConversionClick(object sender, RoutedEventArgs e)
    {
        TSMLuaConvWindow tSMLuaConvWindow = new();
        tSMLuaConvWindow.Show();
    }
    private void BtnSelectCombineCsvsClick(object sender, RoutedEventArgs e)
    {
        CombineCSVsWindow combineCSVsWindow = new();
        combineCSVsWindow.Show();
    }

    private void BtnSelectCreateDBClick(object sender, RoutedEventArgs e)
    {
        DatabaseMain.DataBaseMain();
    }

    private void BtnSelectImportToDBClick(object sender, RoutedEventArgs e)
    {
        ImportCsvsToDatabaseWindow importCsvsToDatabaseWindow = new();
        importCsvsToDatabaseWindow.Show();
    }
}