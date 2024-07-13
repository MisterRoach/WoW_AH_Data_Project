using Serilog;
using System.Globalization;
using System.Windows;
using WoWAHDataProject.Code;
using WoWAHDataProject.Database;
using WoWAHDataProject.GUI;
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
            .WriteTo.File("logs/main_log_.txt", formatProvider: CultureInfo.CurrentCulture, rollingInterval: RollingInterval.Day)
            .WriteTo.File("logs/error_log_.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,formatProvider: CultureInfo.CurrentCulture, rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Information($"Timestamp for current log session: {DateTime.Now}");
        InitializeComponent();

        try
        {
            Egg.プロ生ちゃんNumber();
            Log.Information("Miku threw a: " + Egg.えれくとりっく_えんじぇぅ);
            if(Egg.音)
            {
                Egg.Hatch芸術プロ生ちゃんEgg(this);
            }
        }
        catch (Exception ex)
        {
            Log.Information("Awww it failed", ex);
        }
        finally
        {
        }

        // Check .config file for values and set them if they are not set in case of first launch or update them if needed
        try
        {
            ConfigurationHelper.InitConfigCheck();
        }
        catch
        (Exception ex)
        {
            ExceptionHandling.ExceptionHandler("MainWindow->Tried InitConfigCheck", ex);
        }
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

    private void BtnSelectImportToDatabaseClick(object sender, RoutedEventArgs e)
    {
        ImportToDatabaseMainWindow importToDatabaseMainWindow = new();
        importToDatabaseMainWindow.Show();
    }
}