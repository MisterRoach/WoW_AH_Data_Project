namespace WoW_AH_Data_Project.Code;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
/// <summary>
/// Class that contains method for combining cvs
/// </summary>
public class Combine_Sales_Purchases_Csv
{
    /// <summary>
    /// Method for combining Accounting_*_purchases.csv and Accounting_*_sales.csv
    /// </summary>
    /// <param name="purchases_csv_path">Variable holding path to Accounting_*_purchases.csv file</param>
    /// <param name="sales_csv_path">Variable holding path to Accounting_*_sales.csv file</param>
    /// <param name="output_csv_path">Variable holding path for output file</param>
    public static void Combine_Sales_Purchases_Csvs(string purchases_csv_path, string sales_csv_path, string output_csv_path)
    {
        // Variable we store the CultureInfo value of the system in because we want numbers
        // with a dot . as decimal separator by default and replace the dot . later if the systems default CultureInfo value makes it using comma ,
        var culture = CultureInfo.CurrentCulture.Name;
        try
        {
            // Log the systems CultureInfo value
            Functions.Log($"Original System's CultureInfo Setting is {culture}.");
            // Test double to see how the systems CultureInfo value affects the separator
            double dot_test_num = 100.0001;
            // Make a MatchCollection variable which's content increases if the following Regex
            // that checks if the above double variable contains a dot does return something, need to convert the double variable to string for it
            MatchCollection dotMatch = Regex.Matches(dot_test_num.ToString(), @"\.");
            // Bool variable that's set to false on default
            bool boolDotSep = false;
            // If the Regex.Match found something and increased the content of the dotMatch variable
            // then the .Count method will return a number greater than 0
            // indicating that the system uses dot . as decimal separator
            if (dotMatch.Count > 0)
            {
                // Log info
                Functions.Log("System uses dot as decimal separator!");
                // Set the bool variable to true for later usage to decide if we replace dots . with commas as decimal separator
                boolDotSep = true;
            }
            // If the Regex.Match found nothing, the system uses comma as decimal separator, indicating that we want to change that for now
            else
            {
                // Set the CurrentInfo value of the system to en-EN for now, ensuring that our code produces numbers with a dot . as decimal separator
                // setting the Boolean value to false so that it takes our input
                CultureInfo.CurrentCulture = new CultureInfo("en-EN", false);
                // Log info
                Functions.Log($"Setting system's CultureInfo to {CultureInfo.CurrentCulture.Name} to have a temporary set standard for the decimal separator. Switching back later.");
            }
            // Reading the content of the two csv files into variables
            var purchases_csv_content = File.ReadAllText(purchases_csv_path);
            var sales_csv_content = File.ReadAllText(sales_csv_path);

            // Making a IEnumerable<string> variable / a linq query we use to get the data we want in the way we want
            // beginning by splitting the content of the purchases.csv line by line
            var purchases_query = purchases_csv_content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                // split each line on a comma, returns `IEnumerable<string[]> (thanks to ZZZZZZZZZZZZZZZZZZZZZZZZZ for helping me here)
                .Select(r => r.Split(','))
                // take each `string[]` and creates a tuple from it, returns `IEnumerable<(string itemString, string itemName, int quantity, int price)>
                // (thanks to ZZZZZZZZZZZZZZZZZZZZZZZZZ for helping me here)
                // we also skip the first row that contains the headers of the csv, parse the quantity and price values as decimals
                // and divide the price by 10000, then multiplying by the quantity to get the actual value the item sold for if we sold more then one at a time
                // we divide because otherwise the result would be i.E 7534000 which would be the amount of copper we earned
                // through the division the example gets spit out as 75.34, meaning we earned 75 gold and 34 silver
                .Select(r => (itemString: r[0], itemName: r[1], quantity: decimal.Parse(r[3]), price: (decimal.Parse(r[4]) / 10000) * decimal.Parse(r[3]))).Skip(1)
                // groups the `Ienumerable` of tuples into an `IEnumerable` of groupings (thanks to ZZZZZZZZZZZZZZZZZZZZZZZZZ for helping me here)
                .GroupBy(r => new { r.itemString, r.itemName })
                // Takes that `IEnumerable` of groupings, and based on each grouping creates a tuple (thanks to ZZZZZZZZZZZZZZZZZZZZZZZZZ for helping me here)
                // we sum the quantity and price for all the entries of items we group to get total values for the items
                // we're also turning the whole Select output into a string and trim the brackets ( at the start and end ) away
                .Select(g => (itemString: g.Key.itemString, itemName: g.Key.itemName, quantity: g.Sum(f => f.quantity), price: g.Sum(i => i.price)).ToString().Trim('(', ')'));
            // Now making a query for the sales.csv, basically doing the same like above, but I wrote it in sql-like syntax to try to learn
            var sales_query =
                from split in sales_csv_content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Skip(1)
                select split.Split(",") into f
                select (itemString: f[0], itemName: f[1], quantity: decimal.Parse(f[3]), price: (decimal.Parse(f[4]) / 10000) * decimal.Parse(f[3])) into g
                group g by (g.itemString, g.itemName) into grouped
                select (itemString: grouped.Key.itemString, itemName: grouped.Key.itemName, quantity: grouped.Sum(f => f.quantity), price: grouped.Sum(i => i.price)).ToString().Trim('(', ')');

            // Making a StringBuilder type object to store the following data for the purchases output
            var purchases_csv = new StringBuilder();
            // Adding line with headers to the StringBuilder object using semicolon ; as separator
            // if we used comma , price values with decimal places would get split on 2 columns if we need to use comma , as decimal separator
            purchases_csv.AppendLine("itemString; itemName; quantity; price");
            // Making an empty list object we store data in and use it later in a query to join this list with a list of the sales data
            var purchases_data_list = new List<(string itemString, string itemName, string quantity, string price)>
            {
            };
            // Integer variable to count within the foreach loops
            // keeping track on what index of an array we are
            int iCounter = 0;
            // Iterating through the entries of the purchases data query
            // for each entry/row 
            foreach (var entry in purchases_query)
            {
                // Split the entry/row on comma , and store the results,
                // basically the values inside the rows different columns in form of an array in a variable
                // that becomes an array itself, holding the different results/columns on it's several indexes
                var entry_split = entry.Split(',');
                // Add the values of the itemString, itemName, quantity and price to the purchases data list
                // since the csv file are build in a fixed way, we now on which indexes the values are stored
                purchases_data_list.Add((entry_split[0], entry_split[1], entry_split[2], entry_split[3]));
                // For each part of the split entry/row, basically for the data of each column of a row
                foreach (var entry_split_part in entry_split)
                {
                    // Check
                    // if our counter equals 3, indicating that we are on the column/index of the array that stores the price value AND
                    // if the bool variable that indicates if the system uses dot . decimal separator is set to false
                    if (iCounter == 3 && boolDotSep == false)
                    {
                        // Make a variable and store the value thats on the current column/index of the array we currently iterate through
                        // replacing the dot . decimal separator, "forced" by our fixed set CultureInfo value with a comma ,
                        // the returned result automatically becomes a string
                        var dotToComma = entry_split_part.Replace(".", ",");
                        // Store that value with the replaced decimal separator in the array thats "holding the columns" on the index having the value of our counter(3)
                        // basically: store the value with replaced separator on it's corresponding column
                        entry_split[iCounter] = dotToComma;
                    }
                    // Increase our counter by 1
                    iCounter++;
                }
                // Append a line on our StringBuilder object with the content/columns of the splitted row joined back together, using a semicolon as separator
                purchases_csv.AppendLine(String.Join(";", entry_split));
            }
            // If we finished the iterations, write the data of the Stringbuilder object as a String in the corresponding output file
            File.WriteAllText(@$"{output_csv_path}\purchases_query_output.csv", purchases_csv.ToString());

            // Now all that stuff for the sales data again
            var sales_data_list = new List<(string itemString, string itemName, string quantity, string price)>
            {
            };

            var sales_csv = new StringBuilder();
            sales_csv.AppendLine("itemString; itemName; quantity; price");
            foreach (var entry in sales_query)
            {
                var entry_split = entry.Split(',');
                sales_data_list.Add((entry_split[0], entry_split[1], entry_split[2], entry_split[3]));
                iCounter = 0;
                foreach (var item in entry_split)
                {
                    if (iCounter == 3 && boolDotSep == false)
                    {
                        var dotToComma = item.Replace(".", ",");
                    }
                    iCounter++;
                }
                sales_csv.AppendLine(String.Join(";", entry_split));
            }
            File.WriteAllText(@$"{output_csv_path}\sales_query_output.csv", sales_csv.ToString());

            // Now we get to the part where we combine the data of the files
            // Creating a StringBuilder object again
            var profits_csv = new StringBuilder();
            // Append headers on top
            profits_csv.AppendLine("itemString; itemName; Quantity purchased; Average cost per unit; Total cost; Quantity sold; Average income per unit; Total income; Profit per unit; Total profit");
            // Query for profits
            var profits_query =
                // We take the data from our sales_data_list as sales_data
                from sales_data in sales_data_list
                    // Join the data from our puchases_data_list "where" the itemNames are the same
                join purchases_data in purchases_data_list on sales_data.itemName equals purchases_data.itemName
                // Then we select the itemString and itemName columns from our sales_data
                // we also select the columns in both lists/"tables" storing:
                // the price we bought items for as well as the price we sold them for
                // the quantity we bought of an item and sold of an item
                // we also calculate and add the average income and cost per item by dividing total income/costs for an as many times we sold/bought it
                // then the profit per unit gets calculated and added by subtracting the average income per item by the average cost per item
                // finally we calculate the total profit each item made by taking the Min Value of the sold/bought quantity and multiply the result by the profit per item
                // we also round that last 4 results to 4 decimal places
                select new
                {
                    sales_data.itemString,
                    sales_data.itemName,
                    purchases_price = decimal.Parse(purchases_data.price),
                    sales_price = decimal.Parse(sales_data.price),
                    quantity_purchased = purchases_data.quantity,
                    quantity_sold = sales_data.quantity,
                    average_income_per_unit = Math.Round(decimal.Parse(sales_data.price) / decimal.Parse(sales_data.quantity), 4),
                    average_cost_per_unit = Math.Round(decimal.Parse(purchases_data.price) / decimal.Parse(purchases_data.quantity), 4),
                    profit_per_unit = Math.Round((decimal.Parse(sales_data.price) / decimal.Parse(sales_data.quantity)) - (decimal.Parse(purchases_data.price) / decimal.Parse(purchases_data.quantity)), 4),
                    profit = Math.Round((Math.Min(decimal.Parse(sales_data.quantity), decimal.Parse(purchases_data.quantity))) * ((decimal.Parse(sales_data.price) / decimal.Parse(sales_data.quantity)) - (decimal.Parse(purchases_data.price) / decimal.Parse(purchases_data.quantity))), 4)
                };
            // Foreach entry that we have in the result of the profits query
            foreach (var entry in profits_query)
            {
                // Check if we need to replace the dot . separator
                if (boolDotSep == false)
                {
                    // If so, append the current rows of the entry we want to the StringBuilder and replace the dots . as well
                    profits_csv.AppendLine(entry.itemString + ";" + entry.itemName + ";" + entry.quantity_purchased + "; " + entry.average_cost_per_unit.ToString().Replace(".", ",") + "; " + entry.purchases_price.ToString().Replace(".", ",") + ";" + entry.quantity_sold + "; " + entry.average_income_per_unit.ToString().Replace(".", ",") + "; " + entry.sales_price.ToString().Replace(".", ",") + "; " + entry.profit_per_unit.ToString().Replace(".", ",") + "; " + entry.profit.ToString().Replace(".", ","));
                }
                else
                {
                    // Else, just append the rows
                    profits_csv.AppendLine(entry.itemString + ";" + entry.itemName + ";" + entry.quantity_purchased + "; " + entry.average_cost_per_unit.ToString() + "; " + entry.purchases_price.ToString() + ";" + entry.quantity_sold + "; " + entry.average_income_per_unit.ToString() + "; " + entry.sales_price.ToString().Replace(".", ",") + "; " + entry.profit_per_unit.ToString() + "; " + entry.profit.ToString());
                }
            }
            // If we finished the iterations, write the data of the Stringbuilder object as a String in the corresponding output file
            File.WriteAllText(@$"{output_csv_path}\profits.csv", profits_csv.ToString());
            // Set the CultureInfo value back to the systems standard
            CultureInfo.CurrentCulture = new CultureInfo(culture, false);
        }
        catch (Exception e)
        {
            // Set the CultureInfo value back to the systems standard
            CultureInfo.CurrentCulture = new CultureInfo(culture, false);
            Functions.Log($"Set systems CultureInfo value back to {culture}.");
            // Call my ExceptionHandler and pass the exception to it
            ExceptionHandler.MrExceptionHandler(e.ToString());
        }
        finally
        {
            Functions.Log("Executing finally block of Combine_Sales_Purchases_Csv.cs.");
            // Set the CultureInfo value back to the systems standard if it's not already
            if (CultureInfo.CurrentCulture.Name != culture)
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture, false);
                Functions.Log($"Set systems CultureInfo value back to {culture}.");
            }
        }
    }
}
