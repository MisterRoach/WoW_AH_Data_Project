using System.IO;
using System.Text;

namespace WoW_AH_Data_Project.Code;

public class Functions
{
    // Function to add text into files
    public static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    // Function for logging
    public static void Log(string file_path, string input)
    {
        using (StreamWriter sw = File.AppendText(file_path))
        {
            sw.WriteLine(input);
            sw.Close();
        }
    }

}