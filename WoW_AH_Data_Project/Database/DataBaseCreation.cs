using Microsoft.Data.Sqlite;
using Serilog;
using System.IO;

namespace WoWAHDataProject.Database;
public static class DataBaseCreation
{
    public static void CreateDatabase(string dbFilePath, string dbDirectory, string connString)
    {
        try
        {
            Log.Information("Trying to create database directory.");
            Directory.CreateDirectory(dbDirectory);
            Log.Information($"Created database directory: {dbDirectory}");
            Directory.CreateDirectory(dbDirectory+@"\csv_archive");
            Log.Information("Trying to create database file.");
            using (File.Create(dbFilePath)) { }
            Log.Information($"Created database file: {dbFilePath}");
            using var connection = new SqliteConnection(connString);
            Log.Information("Trying to open database connection.");
            connection.Open();
            Log.Information("Opened database connection.");
            CreateTables(connection);
            Log.Information("Database created.");
            Log.Information("Closing database connection.");
            connection.Close();
        }
        catch (Exception ex)
        {
            Log.Error("Error creating database: {Message}", ex.Message);
            throw;
        }
    }

    public static void CreateTables(SqliteConnection connection)
    {
        Log.Information("Trying to create tables.");
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS
                purchases (
                    purchaseId INTEGER PRIMARY KEY,
                    itemString TEXT,
                    itemName TEXT,
                    stackSize INTEGER,
                    quantity INTEGER,
                    price REAL,
                    otherPlayerId INTEGER,
                    playerId INTEGER,
                    time TEXT,
                    source TEXT,
                    FOREIGN KEY (otherPlayerId) REFERENCES otherPlayer(otherPlayerId),
                    FOREIGN KEY (playerId) REFERENCES player(playerId),
                    UNIQUE (itemString, itemName, stackSize, quantity, price, otherPlayerId, playerId, time, source)
                );
                CREATE TABLE IF NOT EXISTS
                sales (
                    orderId INTEGER PRIMARY KEY,
                    itemString TEXT,
                    itemName TEXT,
                    stackSize INTEGER,
                    quantity INTEGER,
                    price REAL,
                    otherPlayerId INTEGER,
                    playerId INTEGER,
                    time TEXT,
                    source TEXT,
                    FOREIGN KEY (otherPlayerId) REFERENCES otherPlayer(otherPlayerId),
                    FOREIGN KEY (playerId) REFERENCES player(playerId),
                    UNIQUE (itemString, itemName, stackSize, quantity, price, otherPlayerId, playerId, time, source)
                );
                CREATE TABLE IF NOT EXISTS
                otherPlayer (
                    otherPlayerId INTEGER PRIMARY KEY,
                    otherPlayerName TEXT UNIQUE
                );
                CREATE TABLE IF NOT EXISTS
                player (
                    playerId INTEGER PRIMARY KEY,
                    playerName TEXT UNIQUE
                );

                CREATE INDEX idx_sales_otherPlayerId ON sales(otherPlayerId);
                CREATE INDEX idx_purchases_otherPlayerId ON purchases(otherPlayerId);
                CREATE INDEX idx_sales_playerId ON sales(playerId);
                CREATE INDEX idx_purchases_playerId ON purchases(playerId);
                ";
            command.ExecuteNonQuery();

            bool tableName = TableExists(connection, "sales");
            Log.Information($"sales table exists: {tableName}");
            tableName = TableExists(connection, "purchases");
            Log.Information($"purchases table exists: {tableName}");
            tableName = TableExists(connection, "otherPlayer");
            Log.Information($"otherPlayer table exists: {tableName}");
            tableName = TableExists(connection, "player");
            Log.Information($"player table exists: {tableName}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error creating tables: {ex.Message}");
            throw;
        }
    }
    public static bool TableExists(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
        var result = command.ExecuteScalar();
        return result != null;
    }
}