using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace WoW_AH_Data_Project.Code
{
    public class Combine_Sales_Purchases_Csv
    {
        public static void Combine_Sales_Purchases_Csvs(string purchases_csv_path, string sales_csv_path, string output_csv_path)
        {
            try
            {
                //deedle test
                // Make Deedle DataFrame of the purchases.csv
                //var test = Frame.ReadCsv(purchases_csv_path);
                //test.Print();
                //Frame<string, string> res = test.PivotTable<string, string, int, int, int, string, string, int, string>("Sex", "Survived", a => a.GetColumn<int>("Age").ValueCount);
                //Frame<string, string> res = test.PivotTable<int, string, string, string, int>("Sex", "Survived", a => a.GetColumn<int>("Age").ValueCount);

                // Open and read the tsm lua file, it then gets closed automatically
                /*string[] stringArray = File.ReadAllLines(purchases_csv_path);
                int i = 0;
                foreach (var entry in stringArray)
                {
                    i = i + 1;
                    Console.WriteLine("Line Nr:" + i + entry + "\n");
                }*/

                //linqtest
                string[] lines = File.ReadAllLines(purchases_csv_path);


            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
            finally 
            {
                Console.WriteLine("Executing finally block of Combine_Sales_Purchases_Csv.cs.");
            }
        }
    }
}
