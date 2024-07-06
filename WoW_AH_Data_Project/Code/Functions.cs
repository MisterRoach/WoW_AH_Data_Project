namespace WoWAHDataProject.Code;
using System.IO;
using System.Reflection;
using System.Text;
public static class Functions
{
    public static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }
    public static void Log(string input)
    {
        using StreamWriter sw = File.AppendText(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.txt");
        sw.WriteLine(input);
        sw.Close();
    }
}