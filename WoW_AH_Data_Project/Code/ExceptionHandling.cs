using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Serilog;
using WinForms = System.Windows.Forms;

namespace WoWAHDataProject.Code;
public static class ExceptionHandling
{
    public static async void ExceptionHandler(string catchPlace, Exception exception)
    {
        // Log much about it and make it easy destinguishable from other entries
        Log.Error
            ($"\n----------------------------------------------" +
            $"\n----------------------------------------------" +
            $"\n----------------------------------------------" +
            $"\n--------------START OF EXCEPTION---------------" +
            $"\nTime of exception: {DateTime.Now}" +
            $"\nExceptionMessage: {exception.Message}" +
            $"\nCatchBlock that called: {catchPlace}" +
            $"\nDetails of exception:\n{exception}" +
            $"\nInnerException: {exception.InnerException}" +
            $"\nBaseException: {exception.GetBaseException}" +
            $"\nStackTrace: {exception.StackTrace}" +
            $"\nSource: {exception.Source}" +
            $"\nActualMethodThatThrew: {exception.TargetSite}" +
            $"\n---------------END OF EXCEPTION---------------" +
            $"\n----------------------------------------------");
        var excScanResults = ExceptionScanner.ExceptionScan(exception);
        Log.Error($"ExceptionScanner Result: {excScanResults}");
        WinForms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
        WinForms.Application.EnableVisualStyles();
        WinForms.Application.SetCompatibleTextRenderingDefault(true);
        // Cutting contents so they fit in the upcoming dialog
        var regex = new Regex(@".{75}");
        string exInnerEx = exception.InnerException == null ? "No inner exception" : regex.Replace(exception.InnerException.ToString(), "$&" + Environment.NewLine);
        string exStackTrace = exception.StackTrace == null ? "No StackTrace" : regex.Replace(exception.StackTrace, "$&" + Environment.NewLine);
        string exDescription = regex.Replace(excScanResults.Item2, "$&" + Environment.NewLine);
        /*
        if (Egg.音)
        {
            Log.Information(Egg.音ミク失敗[0].Item1);

            await Egg.Play音楽ゲーム(Egg.音ミク失敗[0].Item2, Egg.音ミク失敗[0].Item1);
        }
        */
        TaskDialogButton exDialogResult = TaskDialog.ShowDialog(new TaskDialogPage()
        {
            // exception.message, hoping it's not producing a wall of text
            Text = exDescription,
            // Contains whatever I came up with in ExceptionScanner method
            Caption = excScanResults.Item1,
            Icon = TaskDialogIcon.Error,
            Buttons =
                {
                    TaskDialogButton.OK,
                    "View error log"
                },
            Expander = new TaskDialogExpander()
            {
                // Wall of text goes here
                Text = "InnerException:\n" + exInnerEx + "\nStackTrace:\n " + exStackTrace,
                CollapsedButtonText = "Show details",
                ExpandedButtonText = "Hide details",
                Position = TaskDialogExpanderPosition.AfterFootnote,
            },
            Footnote = "Feel welcome to inform Mr. Roach about errors and issues while using the application\n" +
                        "either via Discord\n" +
                        "or via github.com/MisterRoach/WoW_AH_Data_Project."
        });
        if (exDialogResult != TaskDialogButton.OK)
        {
            if (exDialogResult.ToString() == "View error log")
            {
                Log.Information("User clicked 'Get me to the log' on the exception dialog");
                using Process openLogFile = new();
                {
                    openLogFile.StartInfo.FileName = "notepad.exe";
                    openLogFile.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory + Directory.GetFiles("logs", "*error*").ToList().OrderByDescending(File.GetLastWriteTime).First();
                    openLogFile.Start();
                }
            }
        }
    }
}
