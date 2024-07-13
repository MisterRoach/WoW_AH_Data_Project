namespace WoWAHDataProject.Code;
using System.IO;
using System.Text;
using Serilog;
using System.Globalization;

public static class TSMLuaConverter
{
    public static void TSMLuaAHValuesConverter(string tsmLuaFilePath, string csvOutputPath)
    {
        try
        {
            string[] entrySubparts;
            string[] stringArray = File.ReadAllLines(tsmLuaFilePath);
            string recentMvLine = stringArray[1];
            string regularMvLine = stringArray[2];
            string[] seperators = ["},{", "{{", "}}"];
            string[] dltimeseps = ["downloadTime=", ","];
            string[] recentMvSubparts = recentMvLine.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            string[] regularMvSubparts = regularMvLine.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            int subStart = recentMvLine.IndexOf("{downloadTime=", StringComparison.CurrentCulture) + "{downloadTime=".Length;
            int subEnd = recentMvLine.LastIndexOf(",fields", StringComparison.CurrentCulture);
            //{recentMvLine.Substring(subStart, subEnd - subStart)}
            StringBuilder csvOutPut = new();
            using (FileStream fs = File.Create(csvOutputPath + @"\TSM_recent_market_values.csv"))
            {
                csvOutPut.AppendLine($"itemString;MinBuyout;NumberofAuctions;RecentMarketValue");
                foreach (var entry in recentMvSubparts)
                {
                    if (entry == recentMvSubparts[0] || entry == recentMvSubparts[^1]) continue;
                    entrySubparts = entry.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    // Convert the base32hex numbers we got from the TSM lua file into decimal numbers using the GFG Class from https://www.geeksforgeeks.org/convert-base-decimal-vice-versa/
                    decimal minbuyoutInDec = GFG.ToDeci(entrySubparts[1], 32);
                    decimal numOfAuctionsInDec = GFG.ToDeci(entrySubparts[2], 32);
                    decimal recentMvsInDec = GFG.ToDeci(entrySubparts[3], 32);
                    csvOutPut.AppendLine("i:" + entrySubparts[0] + ";" + (minbuyoutInDec / 10000) + ";" + numOfAuctionsInDec + ";" + (recentMvsInDec / 10000));
                }
                fs.Dispose();
                File.WriteAllText(csvOutputPath + @"\TSM_recent_market_values.csv", csvOutPut.ToString());
                csvOutPut.Clear();
            }
            using (FileStream fs = File.Create(csvOutputPath + @"\TSM_regular_market_values.csv"))
            {
                csvOutPut.AppendLine("itemString;MarketValue");
                foreach (var entry in regularMvSubparts)
                {
                    if (entry == regularMvSubparts[0] || entry == regularMvSubparts[^1]) continue;
                    entrySubparts = entry.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    decimal mvNumbersDecimal = GFG.ToDeci(entrySubparts[1], 32);
                    csvOutPut.AppendLine("i:" + entrySubparts[0] + ";" + (mvNumbersDecimal / 10000));
                }
                fs.Dispose();
                File.WriteAllText(csvOutputPath + @"\TSM_regular_market_values.csv", csvOutPut.ToString());
                csvOutPut.Clear();
            }
        }
        catch (Exception ex)
        {
            Log.Information("Exception: " + ex.Message);
            ExceptionHandling.ExceptionHandler("TSMLuaAHValuesConverter", ex);
        }
        finally
        {
            Console.WriteLine("Executing finally block of TSM_Lua_To_csv.cs.");
        }
    }
}