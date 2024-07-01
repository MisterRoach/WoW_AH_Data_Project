namespace WoW_AH_Data_Project.Code;
using System;
using System.Text.RegularExpressions;


public class ExceptionScanner
{
    public static string MrExceptionScanner(string exception)
    {
        // Log exception and time of occuring
        Functions.Log($"Exception:\nTime of exception: {DateTime.Now.ToString()}\nDetails of exception:\n{exception}");
        // Placeholder for our own exception description if we recognize something
        string exception_scan_result = "";
        // Check if we find text in exception that points us to something we are aware of
        // UnauthorizedAccessException means we most likely have no access to a path we want to access for writing our outputs to
        MatchCollection UAE = Regex.Matches(exception, @"(UnauthorizedAccessException)(.*)(output.cs.*is denied)");
        // Could not find file while pointing towards an output.csv most likely means the same as UnauthorizedAccessException
        // still have to figure out why it shows this instead of the other one, secruity just says folder access blocked like on the other exception
        MatchCollection notFF = Regex.Matches(exception, @"(Could not find file)(.*output\.csv)"); //May have to check if it reacts to UAE aswell

        // Check if we find a known exception and if so, set own description in addition to the regular exception text
        // Look if we got a match when checking for "UnauthorizedAccessException" and or "output.cs.* is denied"
        if (UAE.Count > 0)
        {
            exception_scan_result = "Look's like access to the output path got denied.\n Try to choose another path, allow the action in Windows Security\n or run this file as administrator.";
        }
        // Look if we got a match for "Could not find file" and or with ".*output.csv"
        if (notFF.Count > 0)
        {
            exception_scan_result = "Look's like a output.csv file wasn't found, this probable means it couldn't be created due to\n" +
                        "an UnauthorizedAccessException. Try to choose another path, allow the action in Windows Security\nor run this file as administrator.";
        }
        // Return the result
        return exception_scan_result;
    }
}
