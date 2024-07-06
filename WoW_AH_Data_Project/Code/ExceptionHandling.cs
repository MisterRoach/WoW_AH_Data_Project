namespace WoWAHDataProject.Code;
using System;
using WinForms = System.Windows.Forms;

public static class ExceptionHandling
{
    public static void ExceptionHandler(string exception)
    {
        Functions.Log($"Exception:\nTime of exception: {DateTime.Now}\nDetails of exception:\n{exception}");
        string excScanResult = ExceptionScanner.ExceptionScan(exception);
        Functions.Log($"ExceptionScanner Result: {excScanResult}");
        DialogResult dialogResult = WinForms.MessageBox.Show($"Exception: {excScanResult}\nDetails: {exception}", "Exception", MessageBoxButtons.OK);
        if (dialogResult == WinForms.DialogResult.OK)
        {
            return;
        }
    }
}
