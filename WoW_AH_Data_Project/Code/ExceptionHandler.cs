namespace WoW_AH_Data_Project.Code;
using System;
using WinForms = System.Windows.Forms;

public class ExceptionHandler
{
    public static void MrExceptionHandler(string exception)
    {
        // Log exception and time of occuring
        Functions.Log($"Exception:\nTime of exception: {DateTime.Now.ToString()}\nDetails of exception:\n{exception}");
        // Call the ExceptionScanner to look if we know the exception
        string exception_scanner_result = ExceptionScanner.MrExceptionScanner(exception);
        Functions.Log($"ExceptionScanner Result: {exception_scanner_result}");
        // Make Dialogresult object(?) for user
        DialogResult dialog_result;
        // Display the actual MessageBox containing the exception_scanner_result and the regular exception message for the user
        dialog_result = WinForms.MessageBox.Show($"Exception: {exception_scanner_result}\nDetails: {exception}", "Exception", MessageBoxButtons.OK);
        if (dialog_result == WinForms.DialogResult.OK)
        {
            // Close exception message when "ok" is pressed
            return;
        }
    }
}
