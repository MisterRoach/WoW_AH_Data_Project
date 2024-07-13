namespace WoWAHDataProject.Code;
using System;


public static class ExceptionScanner
{
    public static Tuple<string, string> ExceptionScan(Exception exception)
    {
        string exString = exception.ToString();
        string excScanResult;
        string excScanResultCaptation;
        if (exString.Contains("UnauthorizedAccessException"))
        {
            excScanResultCaptation = "UnauthorizedAccessException";
            excScanResult = "Access to a path trying to read/write into got denied.\n Try to choose another path, allow the action in Windows Security\n or run this file as administrator.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exception.StackTrace.Contains("ConfigurationCheck.SaveAndRefresh"))
        {
            excScanResultCaptation = "Configuration File Error";
            excScanResult = "Look's like an error occured while trying to read/write .config file values\n" +
                            "most likely the application folder got moved and it now lacks read/write permissions.\n" +
                            "Consider granting the app permission to access it's current folder\n" +
                            "or start the app as admin.\n" +
                            "If the above solution doesn't work and/or if everything worked correctly on last usage\n" +
                            "and nothing changed since then consider sharing the issue with Mr. Roach on Discord\n" +
                            "or on Github .";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("DirectoryNotFound") || exString.Contains("FileNotFound") || exString.Contains("Could not find a part of the path"))
        {
            int subStart = exString.IndexOf("'", StringComparison.CurrentCulture) + "'".Length;
            int subEnd = exString.LastIndexOf("'.", StringComparison.CurrentCulture);
            excScanResultCaptation = "Path not found";
            excScanResult = "Could not find the targeted path:\n" +
                            $"'{exString.Substring(subStart, subEnd - subStart)}'\n" +
                            $"The file, directory or path in general does not exist.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("NullReferenceException"))
        {
            excScanResultCaptation = "NullReferenceException";
            excScanResult = "The code stumbled upon a NULL Reference Exception.\n" +
                            "Consider sharing the details of this exception\n" +
                            "with Mr. Roach.\n" +
                            "Without further reference, it's hard to tell where\n" +
                            "and why it happened.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("IndexOutOfRangeException"))
        {
            excScanResultCaptation = "IndexOutOfRangeException";
            excScanResult = "The code tried to access an index that does not exist.\n" +
                            "Like opening a book on page 300 when it only has 200.\n" +
                            "Consider sharing the details of this exception\n" +
                            "with Mr. Roach.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("IOException"))
        {
            excScanResultCaptation = "IOException";
            excScanResult = "An Input/Output error occured.\n" +
                            "This can happen when the code tries to read/write a file\n" +
                            "that is currently in use by another application or\n" +
                            "it does not have access to.\n" +
                            "Consider closing the file in question or the application\n" +
                            "that uses it and try again.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("StackOverflowException"))
        {
            excScanResultCaptation = "StackOverflowException";
            excScanResult = "Maybe the code got stuck in an infinite loop.\n" +
                            "Definetly consider sharing the details of this exception\n" +
                            "with Mr. Roach.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("ArgumentException"))
        {
            excScanResultCaptation = "ArgumentException";
            excScanResult = "A passed parameter for a code's function is invalid.\n" +
                            "Definetly consider sharing the details of this exception\n" +
                            "with Mr. Roach.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("FormatException"))
        {
            excScanResultCaptation = "FormatException";
            excScanResult = "A format of a passed parameter is invalid.\n" +
                            "Definetly consider sharing the details of this exception\n" +
                            "with Mr. Roach.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        if (exString.Contains("OutOfMemoryException"))
        {
            excScanResultCaptation = "OutOfMemoryException";
            excScanResult = "The code ran out of memory.\n" +
                            "Definetly consider sharing the details of this exception\n" +
                            "with Mr. Roach.";
            return new Tuple<string, string>(excScanResultCaptation, excScanResult);
        }
        return new Tuple<string, string>("Ayayayayay! Some exception occured", 
                                        "What ever happend I didn't wrote a customized message for it yet." +
                                        "\nFeel free to share what happened to Mr. Roach.\n" + exception.Message + "\n");
    }
}
