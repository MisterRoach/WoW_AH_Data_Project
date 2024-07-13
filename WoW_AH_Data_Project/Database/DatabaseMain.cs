using System.Configuration;
using Serilog;
using System.IO;

namespace WoWAHDataProject.Database;

static class DatabaseMain
{
    public static string dbDirectory = ConfigurationManager.AppSettings["dbDirectory"];
    public static string dbFilePath = ConfigurationManager.AppSettings["dbFilePath"];
    public static string dbArchivePath = ConfigurationManager.AppSettings["dbArchivePath"];
    public static string dbCsvArchivePath = ConfigurationManager.AppSettings["dbCsvArchivePath"];
    public static string dbLuaArchivePath = ConfigurationManager.AppSettings["dbLuaArchivePath"];
    public static string connString = $"Data Source={dbFilePath}";
    public static void DataBaseMain()
    {
        /*var dbDirectory = ConfigurationManager.AppSettings["dbDirectory"];
        var dbFilePath = ConfigurationManager.AppSettings["dbFilePath"];
        var dbArchivePath = ConfigurationManager.AppSettings["dbArchivePath"];
        var dbCsvArchivePath = ConfigurationManager.AppSettings["dbCsvArchivePath"];
        var connString = $"Data Source={dbFilePath}";*/

        if (!File.Exists(dbFilePath))
        {
            Log.Warning("Database file not found.");
            Log.Information("Trying to create database.");
            DataBaseCreation.CreateDatabase(dbFilePath, dbDirectory, dbArchivePath, dbCsvArchivePath, dbLuaArchivePath, connString);
        }
        else
        {
            Log.Information($"Database file found: {dbFilePath}");
        }
    }
}