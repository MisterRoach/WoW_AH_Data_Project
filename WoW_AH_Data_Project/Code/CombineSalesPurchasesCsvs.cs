namespace WoWAHDataProject.Code;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
public static class CombineSalesPurchasesCsvs
{
    public static void CombineSalesPurchasesCsv(string purchasesCsvPath, string salesCsvPath, string outputCsvPath)
    {
        // Store CultureInfo of system
        //var culture = CultureInfo.CurrentCulture.Name;
        var culture = CultureInfo.InvariantCulture;
        try
        {
            Log.Information($"Original System's CultureInfo Setting is {culture}.");
            // Test double to see how the systems CultureInfo value affects the separator
            const double dotTestNum = 100.0001;
            MatchCollection dotMatch = Regex.Matches(dotTestNum.ToString(), @"\.");
            bool boolDotSep = false;
            // If the Regex.Match found something we know system uses dot as separator
            if (dotMatch.Count > 0)
            {
                Log.Information("System uses dot as decimal separator!");
                boolDotSep = true;
            }
            else
            {
                // Set the CurrentInfo value of the system to en-EN for now, ensuring that our code produces numbers with a dot . as decimal separator
                CultureInfo.CurrentCulture = new CultureInfo("en-EN", false);
                Log.Information($"Setting system's CultureInfo to {CultureInfo.CurrentCulture.Name} to have a temporary set standard for the decimal separator. Switching back later.");
            }
            // Reading the content of the two csv files into variables
            var purchasesCsvContent = File.ReadAllText(purchasesCsvPath);
            var salesCsvContent = File.ReadAllText(salesCsvPath);

            // Making a IEnumerable<string> variable
            var purchasesQuery = purchasesCsvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                // split each line on a comma, returns `IEnumerable<string[]>
                .Select(r => r.Split(','))
                // take each `string[]` and creates a tuple from it, returns `IEnumerable<(string itemString, string itemName, int quantity, int price)>
                .Select(r => (itemString: r[0], itemName: r[1], quantity: decimal.Parse(r[3],culture), price: (decimal.Parse(r[4],culture) / 10000) * decimal.Parse(r[3],culture))).Skip(1)
                // groups the `Ienumerable` of tuples into an `IEnumerable` of groupings
                .GroupBy(r => new { r.itemString, r.itemName })
                // Takes that `IEnumerable` of groupings, and based on each grouping creates a tuple
                .Select(g => (g.Key.itemString, g.Key.itemName, quantity: g.Sum(f => f.quantity), price: g.Sum(i => i.price)).ToString().Trim('(', ')'));

            var salesQuery =
                from split in salesCsvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Skip(1)
                select split.Split(",") into f
                select (itemString: f[0], itemName: f[1], quantity: decimal.Parse(f[3],culture), price: (decimal.Parse(f[4],culture) / 10000) * decimal.Parse(f[3],culture)) into g
                group g by (g.itemString, g.itemName) into grouped
                select (grouped.Key.itemString, grouped.Key.itemName, quantity: grouped.Sum(f => f.quantity), price: grouped.Sum(i => i.price)).ToString().Trim('(', ')');

            var purchasesCsv = new StringBuilder();
            // Adding line with headers to the StringBuilder object using semicolon ; as separator
            // if we used comma , price values with decimal places would get split on 2 columns
            purchasesCsv.AppendLine("itemString; itemName; quantity; price");
            // Making an empty list object we store data in and use it later in a query to join this list with a list of the sales data
            var purchasesDataList = new List<(string itemString, string itemName, string quantity, string price)>();

            int counter = 0;
            foreach (var entry in purchasesQuery)
            {
                var entrySplit = entry.Split(',');
                purchasesDataList.Add((entrySplit[0], entrySplit[1], entrySplit[2], entrySplit[3]));
                foreach (var entrySplitPart in entrySplit)
                {
                    if (counter == 3 && !boolDotSep)
                    {
                        var dotToComma = entrySplitPart.Replace(".", ",");
                        entrySplit[counter] = dotToComma;
                    }
                    counter++;
                }
                purchasesCsv.AppendJoin(";", entrySplit).AppendLine();
            }
            // If we finished the iterations, write the data of the Stringbuilder object as a string in the corresponding output file
            File.WriteAllText(@$"{outputCsvPath}\purchases_query_output.csv", purchasesCsv.ToString());

            var salesDataList = new List<(string itemString, string itemName, string quantity, string price)>();

            var salesCsv = new StringBuilder();
            salesCsv.AppendLine("itemString; itemName; quantity; price");
            foreach (var entry in salesQuery)
            {
                var entrySplit = entry.Split(',');
                salesDataList.Add((entrySplit[0], entrySplit[1], entrySplit[2], entrySplit[3]));
                counter = 0;
                foreach (var entrySplitPart in entrySplit)
                {
                    if (counter == 3 && !boolDotSep)
                    {
                        var dotToComma = entrySplitPart.Replace(".", ",");
                    }
                    counter++;
                }
                salesCsv.AppendJoin(";", entrySplit).AppendLine();
            }
            File.WriteAllText(@$"{outputCsvPath}\sales_query_output.csv", salesCsv.ToString());

            // Now we we combine the data of the files
            var profitsCsv = new StringBuilder();
            // Append headers
            profitsCsv.AppendLine("itemString; itemName; Quantity purchased; Average cost per unit; Total cost; Quantity sold; Average income per unit; Total income; Profit per unit; Total profit");

            var profitsQuery =
                // Select data, join on itemName
                from salesData in salesDataList
                join purchasesData in purchasesDataList on salesData.itemName equals purchasesData.itemName
                select new
                {
                    // Select and make additional columns filled with calculation results
                    salesData.itemString,
                    salesData.itemName,
                    purchasesPrice = decimal.Parse(purchasesData.price),
                    salesPrice = decimal.Parse(salesData.price),
                    quantityPurchases = purchasesData.quantity,
                    quantitySold = salesData.quantity,
                    avgIncomePerUnit = Math.Round(decimal.Parse(salesData.price) / decimal.Parse(salesData.quantity), 4),
                    avgCostPerUnit = Math.Round(decimal.Parse(purchasesData.price) / decimal.Parse(purchasesData.quantity), 4),
                    profitPerUnit = Math.Round((decimal.Parse(salesData.price) / decimal.Parse(salesData.quantity)) - (decimal.Parse(purchasesData.price) / decimal.Parse(purchasesData.quantity)), 4),
                    totalProfit = Math.Round((Math.Min(decimal.Parse(salesData.quantity), decimal.Parse(purchasesData.quantity))) * ((decimal.Parse(salesData.price) / decimal.Parse(salesData.quantity)) - (decimal.Parse(purchasesData.price) / decimal.Parse(purchasesData.quantity))), 4)
                };

            foreach (var entry in profitsQuery)
            {
                if (!boolDotSep)
                {
                    profitsCsv.AppendLine(entry.itemString + "; " + entry.itemName + "; " + entry.quantityPurchases + "; " + entry.avgCostPerUnit.ToString().Replace(".", ",") + "; " + entry.purchasesPrice.ToString().Replace(".", ",") + ";" + entry.quantitySold + "; " + entry.avgIncomePerUnit.ToString().Replace(".", ",") + "; " + entry.salesPrice.ToString().Replace(".", ",") + "; " + entry.profitPerUnit.ToString().Replace(".", ",") + "; " + entry.totalProfit.ToString().Replace(".", ","));
                }
                else
                {
                    profitsCsv.AppendLine(entry.itemString + "; " + entry.itemName + "; " + entry.quantityPurchases + "; " + entry.avgCostPerUnit.ToString() + "; " + entry.purchasesPrice.ToString() + ";" + entry.quantitySold + "; " + entry.avgIncomePerUnit.ToString() + "; " + entry.salesPrice.ToString().Replace(".", ",") + "; " + entry.profitPerUnit.ToString() + "; " + entry.totalProfit.ToString());
                }
            }
            File.WriteAllText(@$"{outputCsvPath}\profits.csv", profitsCsv.ToString());
            // Set the CultureInfo value back to the systems standard
            //CultureInfo.CurrentCulture = new CultureInfo(culture, false);
        }
        catch (Exception ex)
        {
            //CultureInfo.CurrentCulture = new CultureInfo(culture, false);
            Log.Information($"Set systems CultureInfo value back to {culture}.");
            ExceptionHandling.ExceptionHandler("CombineSalesPurchasesCsv.cs->same Method", ex);
        }
        finally
        {
            Log.Information("Executing finally block of Combine_Sales_Purchases_Csv.cs.");
            //if (CultureInfo.CurrentCulture.Name != culture)
            //{
            //    CultureInfo.CurrentCulture = new CultureInfo(culture, false);
            //    Functions.Log($"Set systems CultureInfo value back to {culture}.");
            //}
        }
    }
}
