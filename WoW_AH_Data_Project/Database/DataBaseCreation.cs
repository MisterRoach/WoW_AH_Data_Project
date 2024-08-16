using Microsoft.Data.Sqlite;
using Serilog;
using System.IO;
using WoWAHDataProject.Code;

namespace WoWAHDataProject.Database;

public static class DataBaseCreation
{
    public static async Task CreateDatabase(string dbFilePath, string dbDirectory, string dbArchivePath, string dbCsvArchivePath, string dbLuaArchivePath, string connString)
    {
        try
        {
            Log.Information("Trying to create database directory.");
            Directory.CreateDirectory(dbDirectory);
            Log.Information($"Created database directory: {dbDirectory}");
            Log.Information("Trying to create csv archive directory.");
            Directory.CreateDirectory(dbCsvArchivePath);
            Log.Information("Created csv archive directory.");
            Log.Information("Trying to create csv archive files folder.");
            Directory.CreateDirectory(dbCsvArchivePath + @"\files");
            Log.Information("Created csv archive files folder.");
            Log.Information("Trying to create csv archive compressed files folder.");
            Directory.CreateDirectory(dbCsvArchivePath + @"\files\compressed");
            Log.Information("Created csv archive files folder.");
            Log.Information("Trying to create file to store archived csv hashes.");
            File.Create(dbCsvArchivePath + @"\archived_csv_hashes.txt");
            Log.Information("Created file to store archived csv hashes.");
            Log.Information("Trying to create lua archive files folder.");
            Directory.CreateDirectory(dbLuaArchivePath + @"\files");
            Log.Information("Created lua archive files folder.");
            Log.Information("Trying to create lua archive compressed files folder.");
            Directory.CreateDirectory(dbLuaArchivePath + @"\files\compressed");
            Log.Information("Created lua archive files folder.");
            Log.Information("Trying to create file to store archived csv hashes.");
            File.Create(dbLuaArchivePath + @"\archived_lua_hashes.txt");
            Log.Information("Created file to store archived lua hashes.");
            Log.Information("Trying to create database archive directory.");
            Directory.CreateDirectory(dbArchivePath);
            Log.Information("Created database archive directory.");
            Log.Information("Trying to create database file.");
            using (File.Create(dbFilePath)) { }
            Log.Information($"Created database file: {dbFilePath}");
            using SqliteConnection connection = new(connString);
            Log.Information("Trying to open database connection.");
            await connection.OpenAsync();
            Log.Information("Opened database connection.");
            await CreateTables(connection);
            Log.Information("Database created.");
            Log.Information("Closing database connection.");
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            Log.Error("Error creating database: {Message}", ex.Message);
            ExceptionHandling.ExceptionHandler("DataBaseCreation->CreateDatabase", ex);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public static async Task CreateTables(SqliteConnection connection)
    {
        Log.Information("Trying to create tables.");
        try
        {
            using SqliteCommand command = connection.CreateCommand();
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
                CREATE TABLE IF NOT EXISTS
                recentMarketValues (
                    itemId INTEGER PRIMARY KEY,
                    UNIQUE (itemId)
                );

                CREATE INDEX idx_sales_otherPlayerId ON sales(otherPlayerId);
                CREATE INDEX idx_purchases_otherPlayerId ON purchases(otherPlayerId);
                CREATE INDEX idx_sales_playerId ON sales(playerId);
                CREATE INDEX idx_purchases_playerId ON purchases(playerId);
                CREATE INDEX idx_recentMarketValues_itemId ON recentMarketValues(itemId);
                PRAGMA journal_mode = wal;
                ";
            await command.ExecuteNonQueryAsync();

            bool tableExists = TableExists(connection, "sales");
            Log.Information($"sales table exists: {tableExists}");
            tableExists = TableExists(connection, "purchases");
            Log.Information($"purchases table exists: {tableExists}");
            tableExists = TableExists(connection, "otherPlayer");
            Log.Information($"otherPlayer table exists: {tableExists}");
            tableExists = TableExists(connection, "player");
            Log.Information($"player table exists: {tableExists}");
            tableExists = TableExists(connection, "recentMarketValues");
            Log.Information($"recentMarketValues table exists: {tableExists}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error creating tables: {ex.Message}");
            ExceptionHandling.ExceptionHandler("DataBaseCreation->CreateTables", ex);
        }
    }

    public static bool TableExists(SqliteConnection connection, string tableName)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
        object result = command.ExecuteScalar();
        return result != null;
    }
}