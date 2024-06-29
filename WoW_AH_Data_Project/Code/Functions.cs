namespace WoW_AH_Data_Project.Code;
using System.IO;
using System.Reflection;
using System.Text;

public class Functions
{
    // Function to add text into files
    public static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    // Function for logging
    public static void Log(string input)
    {
        // Variable for path for the log file, basically the path of app execution
        using (StreamWriter sw = File.AppendText(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.txt"))
        {
            sw.WriteLine(input);
            sw.Close();
        }
    }

}