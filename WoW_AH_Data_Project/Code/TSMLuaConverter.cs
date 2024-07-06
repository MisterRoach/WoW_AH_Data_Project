namespace WoWAHDataProject.Code;
using System.IO;

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
            string[] recentMvSubparts = recentMvLine.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            string[] regularMvSubparts = regularMvLine.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            using (FileStream fs = File.Create(csvOutputPath + "\\TSM_recent_market_values.csv"))
            {
                Functions.AddText(fs, "itemString;MinBuyout;NumberofAuctions;RecentMarketValue\n");
                foreach (var entry in recentMvSubparts)
                {
                    if (entry == recentMvSubparts[0] || entry == recentMvSubparts[^1]) continue;
                    entrySubparts = entry.Split(',');
                    // Convert the base32hex numbers we got from the TSM lua file into decimal numbers using the GFG Class from https://www.geeksforgeeks.org/convert-base-decimal-vice-versa/
                    decimal minbuyoutInDec = GFG.ToDeci(entrySubparts[1], 32);
                    decimal numOfAuctionsInDec = GFG.ToDeci(entrySubparts[2], 32);
                    decimal recentMvsInDec = GFG.ToDeci(entrySubparts[3], 32);
                    Functions.AddText(fs, "i:" + entrySubparts[0] + ";" + (minbuyoutInDec / 10000) + ";" + numOfAuctionsInDec + ";" + (recentMvsInDec / 10000) + "\n");
                }
                fs.Close();
            }
            using (FileStream fs = File.Create(csvOutputPath + "\\TSM_regular_market_values.csv"))
            {
                Functions.AddText(fs, "itemString;MarketValue\n");
                foreach (var entry in regularMvSubparts)
                {
                    if (entry == regularMvSubparts[0] || entry == regularMvSubparts[^1]) continue;
                    entrySubparts = entry.Split(',');
                    decimal mvNumbersDecimal = GFG.ToDeci(entrySubparts[1], 32);
                    Functions.AddText(fs, "i:" + entrySubparts[0] + ";" + (mvNumbersDecimal / 10000) + "\n");
                }
                fs.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Executing finally block of TSM_Lua_To_csv.cs.");
        }
    }
}