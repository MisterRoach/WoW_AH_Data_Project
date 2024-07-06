namespace WoWAHDataProject.Code;
using System;
using System.Text.RegularExpressions;

public static class ExceptionScanner
{
    public static string ExceptionScan(string exception)
    {
        Functions.Log($"Exception:\nTime of exception: {DateTime.Now}\nDetails of exception:\n{exception}");
        string excScanResult = "";
        MatchCollection uaeExc = Regex.Matches(exception, "(UnauthorizedAccessException)(.*)(output.cs.*is denied)");
        MatchCollection notFF = Regex.Matches(exception, @"(Could not find file)(.*output\.csv)");

        if (uaeExc.Count > 0)
        {
            excScanResult = "Look's like access to the output path got denied.\n Try to choose another path, allow the action in Windows Security\n or run this file as administrator.";
        }
        if (notFF.Count > 0)
        {
            excScanResult = "Look's like a output.csv file wasn't found, this probable means it couldn't be created due to\n" +
                        "an UnauthorizedAccessException. Try to choose another path, allow the action in Windows Security\nor run this file as administrator.";
        }
        return excScanResult;
    }
}
