using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.IO;
using Serilog;
using Microsoft.Data.Sqlite;

namespace WoWAHDataProject.Database
{
    internal static class DatabaseImportCsvs
    {
        // Declaring how many threads can access the semaphore code part at once
        // ensuring that only one thread can access the database at a time to prevent issues
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        // Delay between retries in case of deadlock in ms and max number of retries
        // used in ExecuteWithRetryAsync
        const int DelayBetweenRetries = 500;
        const int maxRetries = 5;

        private static readonly CsvConfiguration CsvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            ReadingExceptionOccurred = re =>
            {
                Console.WriteLine($"Bad row: {re.Exception}");
                return false;
            }
        };

        // Method started via button press in ImportCsvsToDatabase Window
        // Also using async, Task and await in the code to come so that UI doesn't freeze
        public static async Task DatabaseImportCsvFiles(IEnumerable<string> csvFiles, string connString)
        {
            var tasks = csvFiles.Select(file => ProcessCsvFileAsync(file, connString));
            await Task.WhenAll(tasks);
        }

        public static async Task ProcessCsvFileAsync(string file, string connString)
        {
            Log.Information("File to import: " + file);

            await semaphore.WaitAsync(); // Queue up threads trying to access the database
            try
            {
                await using var connection = new SqliteConnection(connString);
                Log.Information("Trying to open database connection to:");
                Log.Information(connString);

                try
                {
                    await connection.OpenAsync();
                    Log.Information("Opened database connection.");

                    var query = "";
                    // Query snippet that gets append to the actual query depending on the file
                    var insertIntoFields = @" (
                                        itemString,
                                        itemName, 
                                        stackSize, 
                                        quantity, 
                                        price, 
                                        otherPlayerId, 
                                        playerId, 
                                        time, 
                                        source) 
                                    VALUES (
                                        @itemString, 
                                        @itemName, 
                                        @stackSize, 
                                        @quantity, 
                                        @price, 
                                        @otherPlayerId, 
                                        @playerId, 
                                        @time, 
                                        @source);";
                    if (file.Contains("purchases"))
                    {
                        query =         @"INSERT OR IGNORE INTO 
                                        purchases" + insertIntoFields;
                    }
                    else if (file.Contains("sales"))
                    {
                        query =         @"INSERT OR IGNORE INTO 
                                        sales" + insertIntoFields;
                    }
                    // Queries to insert new otherPlayerNames and playerNames into their respective tables
                    var queryOtherPlayer = @"
                                        INSERT INTO otherPlayer (otherPlayerName)
                                        VALUES (@otherPlayerName)
                                        ON CONFLICT(otherPlayerName) DO NOTHING;
                                        SELECT otherPlayerId FROM otherPlayer WHERE otherPlayerName = @otherPlayerName;";
                    var queryPlayer = @"
                                        INSERT INTO player (playerName)
                                        VALUES (@playerName)
                                        ON CONFLICT(playerName) DO NOTHING;
                                        SELECT playerId FROM player WHERE playerName = @playerName;";
                    // Read csv file to list
                    var rows = ReadEntriesFromCSV(file).ToList();
                    await ExecuteWithRetryAsync(async () =>
                    {
                        await using var transaction = await connection.BeginTransactionAsync();
                        // Try to execute the transaction with chance of retrying
                        try
                        {
                            foreach (var row in rows)
                            {
                                // Insert or get otherPlayerId and add (if not already) otherPlayerName to otherPlayer table
                                int otherPlayerId;
                                int playerId;
                                await using (var sqlcmd = new SqliteCommand(queryOtherPlayer, connection, (SqliteTransaction)transaction))
                                {
                                    sqlcmd.Parameters.AddWithValue("@otherPlayerName", row.otherPlayer);
                                    otherPlayerId = Convert.ToInt32(await sqlcmd.ExecuteScalarAsync(), CultureInfo.CurrentCulture);
                                }
                                // Same for the player
                                await using (var sqlcmd = new SqliteCommand(queryPlayer, connection, (SqliteTransaction)transaction))
                                {
                                    sqlcmd.Parameters.AddWithValue("@playerName", row.player);
                                    playerId = Convert.ToInt32(await sqlcmd.ExecuteScalarAsync(), CultureInfo.CurrentCulture);
                                }

                                // Insert new sales/purchases data into their respective tables
                                await using (var sqlcmd = new SqliteCommand(query, connection, (SqliteTransaction)transaction))
                                {
                                    sqlcmd.Parameters.AddWithValue("@itemString", row.itemString);
                                    sqlcmd.Parameters.AddWithValue("@itemName", row.itemName);
                                    sqlcmd.Parameters.AddWithValue("@stackSize", row.stackSize);
                                    sqlcmd.Parameters.AddWithValue("@quantity", row.quantity);
                                    sqlcmd.Parameters.AddWithValue("@price", row.price / 10000 * row.quantity);
                                    sqlcmd.Parameters.AddWithValue("@otherPlayerId", otherPlayerId);
                                    sqlcmd.Parameters.AddWithValue("@playerId", playerId);
                                    sqlcmd.Parameters.AddWithValue("@time", row.time);
                                    sqlcmd.Parameters.AddWithValue("@source", row.source);
                                    await sqlcmd.ExecuteNonQueryAsync();
                                }
                            }
                            await transaction.CommitAsync();
                            Log.Information("Successfully Imported data from: " + file);
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Log.Error("Error during transaction: " + ex);
                            throw;
                        }
                    });
                    Log.Information("Closing database connection.");
                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    Log.Error("Error opening database connection: " + ex);
                    throw;
                }
            }
            finally
            {
                semaphore.Release(); // Release the semaphore object, allowing the next thread to act
            }
        }

        private static async Task ExecuteWithRetryAsync(Func<Task> operation)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6) // 5 = SQLITE_LOCKED, 6 = SQLITE_BUSY
                {
                    retryCount++;
                    Log.Warning($"Deadlock or busy exception detected. Retrying {retryCount}/{maxRetries}...");
                    // Delay between retries, increasing with each retry
                    await Task.Delay(DelayBetweenRetries*retryCount);
                }
                catch (Exception ex)
                {
                    Log.Error("An error occurred: " + ex);
                    throw;
                }
            }
            Log.Error("Failed to complete operation after multiple retries due to deadlocks.");
        }
        // Method for reading csv entries
        public static IEnumerable<AllEntries> ReadEntriesFromCSV(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CsvConfig);

            // Skip header of the csv file
            csv.Read();
            // Read the header of the csv file to map to fields
            csv.ReadHeader();

            while (csv.Read())
            {
                yield return new AllEntries(
                    csv.GetField<string>("itemString"),
                    csv.GetField<string>("itemName"),
                    csv.GetField<int>("stackSize"),
                    csv.GetField<int>("quantity"),
                    csv.GetField<decimal>("price"),
                    csv.GetField<string>("otherPlayer"),
                    csv.GetField<string>("player"),
                    csv.GetField<string>("time"),
                    csv.GetField<string>("source")
                );
            }
        }

        public record AllEntries
        (
            string itemString,
            string itemName,
            int stackSize,
            int quantity,
            decimal price,
            string otherPlayer,
            string player,
            string time,
            string source
        );
    }
}