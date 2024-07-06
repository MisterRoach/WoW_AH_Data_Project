using System.Configuration;
using Serilog;
using System.IO;

namespace WoWAHDataProject.Database;
public static class DatabaseMain
{
    public static void DataBaseMain()
    {
        var dbDirectory = ConfigurationManager.AppSettings["dbDirectory"];
        var dbFilePath = ConfigurationManager.AppSettings["dbFilePath"];
        var connString = $"Data Source={dbFilePath}";

        if (!File.Exists(dbFilePath))
        {
            Log.Warning("Database file not found.");
            Log.Information("Trying to create database.");
            DataBaseCreation.CreateDatabase(dbFilePath, dbDirectory, connString);
        }
        else
        {
            Log.Information($"Database file found: {dbFilePath}");
        }
    }
}