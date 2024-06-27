using System.IO;
using System.Windows;
using System.Reflection;
using TSM_Data_Converter;
using WoW_AH_Data_Project;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TSM_Data_Converter;

public class TSM_Lua_To_csv
{
    public static void TSM_Lua_AH_Values_Converter(string tsm_lua_file_path, string csv_output_path)
    {
        try
        {
            // Variables for the different paths for the files
            // TSM lua file path
            //string tsm_lua_file_path = @"L:\WoW\World of Warcraft\_classic_\Interface\AddOns\TradeSkillMaster_AppHelper\AppData.lua";
            // Recent market values output path
            //string recent_mv_output_path = @"C:\Users\renes\Documents\WoW\AH\csharp\TSM_recent_market_values.csv";
            // Regular market values outputh path
            //string regular_mv_output_path = @"C:\Users\renes\Documents\WoW\AH\csharp\TSM_regular_market_values.csv";

            // String array that gets filled with the parts gotten by splitting the individual entries of the market values files
            string[] entry_subparts;

            // Open and read the tsm lua file, it then gets closed automatically
            string[] stringArray = File.ReadAllLines(tsm_lua_file_path);
            // Make strings for the lines containing the recent and regular/normal market values 
            // Recent market values are located in the second line of the lua file
            string recent_mv_line = stringArray[1];
            // Regular market values are located in the third line of the lua file
            string regular_mv_line = stringArray[2];
            // Creating an array of seperators used to split the single lines
            string[] seperators = { "},{", "{{", "}}" };
            // Creating individual arrays for the two different market values, splitting up the data in the lua file lines into as many pieces it finds
            string[] recent_mv_subparts = recent_mv_line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            string[] regular_mv_subparts = regular_mv_line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            // Create or overwrite file for the recent market values output
            using (FileStream fs = File.Create(csv_output_path + "\\TSM_recent_market_values.csv"))
            {
                // Add headers
                Functions.AddText(fs, "itemString;MinBuyout;NumberofAuctions;RecentMarketValue\n");
                // Loop through all the entries/rows we found in the TSM lua file for the recent ah values 
                foreach (var entry in recent_mv_subparts)
                {
                    // Iterate only through entries/rows that are not the first or the last
                    if (entry == recent_mv_subparts[0] | entry == recent_mv_subparts[^1]) continue;
                    // Split each entry/row, using the comma , in it as split indicator
                    entry_subparts = entry.Split(',');
                    // Convert the base32hex numbers we got from the TSM lua file into decimal numbers using the GFG Class from https://www.geeksforgeeks.org/convert-base-decimal-vice-versa/
                    decimal minbuyout_decimal = GFG.toDeci(entry_subparts[1], 32);
                    decimal numberofauctions_decimal = GFG.toDeci(entry_subparts[2], 32);
                    decimal recentmarketvalue_decimal = GFG.toDeci(entry_subparts[3], 32);
                    // Write the itemString with an i: infront of it and the converted numbers divided by 10000 into the output file using semicolon ; column seperators
                    Functions.AddText(fs, "i:" + entry_subparts[0] + ";" + minbuyout_decimal / 10000 + ";" + numberofauctions_decimal + ";" + recentmarketvalue_decimal / 10000 + "\n");
                }
                // Close the file
                fs.Close();
            }
            // Create or overwrite file for the regular/normal market values output
            using (FileStream fs = File.Create(csv_output_path+ "\\TSM_regular_market_values.csv"))
            {
                // Add headers
                Functions.AddText(fs, "itemString;MarketValue\n");
                // Loop through all the entries/rows we found in the TSM lua file for the regular/normal ah values 
                foreach (var entry in regular_mv_subparts)
                {
                    // Don't iterate through the first and last entry/row
                    if (entry == regular_mv_subparts[0] | entry == regular_mv_subparts[^1]) continue;
                    // Split each entry/row, using the comma , in it as split indicator
                    entry_subparts = entry.Split(',');
                    // Convert the numbers we got from the TSM lua file into decimal numbers
                    decimal marketvalue_decimal = GFG.toDeci(entry_subparts[1], 32);
                    // Write the itemString with an i: infront of it and the converted numbers divided by 10000 into the output file using semicolon ; column seperators
                    Functions.AddText(fs, "i:" + entry_subparts[0] + ";" + marketvalue_decimal / 10000 + "\n");
                }
                // Close the file
                fs.Close();
            }
        }
        //Catch exception if something went wrong
        catch (Exception e)
        {
            //Print the reason for exception to console
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Executing finally block.");
        }

    }
}