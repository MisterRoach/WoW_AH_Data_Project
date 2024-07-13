using Microsoft.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using WoWAHDataProject.Code;

namespace WoWAHDataProject.Database
{
    internal static class DatabaseImportMarketValues
    {
        // Declaring how many threads can access the semaphore code part at once
        // ensuring that only one thread can access the database at a time to prevent issues
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        // Delay between retries in case of deadlock in ms and max number of retries
        // used in ExecuteWithRetryAsync
        const int DelayBetweenRetries = 500;
        const int maxRetries = 5;

        // Method started via button press in ImportMarketValuesToDatabase Window
        // Also using async, Task and await in the code to come so that UI doesn't freeze
        public static async Task<bool> DatabaseImportLuaMarketValues(IEnumerable<string> luaFiles, string connString)
        {
            bool exceptionOccured = false;
            var connection = new SqliteConnection(connString);
            var tasks = luaFiles.Select(file => ProcessLuaFileAsync(file, connection));
            try
            {
                // Open connection execute a Task for amount of files
                await connection.OpenAsync();
                Log.Information("Opened database connection.");
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Error("Error during processing lua files: " + ex);
                ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->DatabaseImportMarketValues->try->WhenAll", ex);
                exceptionOccured = true;
            }
            finally
            {
                // If no exception occured, order columns in the table
                if(!exceptionOccured)
                {
                    Log.Information("Processed all files.");
                    await Task.Run(() => OrderColumns(connection));
                    Log.Information("Ordered Columns.");
                }
                // Close connection
                Log.Information("Closing database connection.");
                await connection.CloseAsync();
            }
            return exceptionOccured;
        }
        public static async Task ProcessLuaFileAsync(string file, SqliteConnection connection)
        {
            string fileToArchive = Path.Combine(DatabaseMain.dbLuaArchivePath + @"\files", Path.GetFileNameWithoutExtension(file)+DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture)+Path.GetExtension(file));
            string compressedFilePath = Path.Combine(DatabaseMain.dbLuaArchivePath + @"\files\compressed", Path.GetFileNameWithoutExtension(file)+DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture));
            // Queue up threads trying to access the database
            await semaphore.WaitAsync();
            Log.Information("File to import: " + file);
            // Check if we got HashCode for the lua file already stored in archive, if not process it, if yes don't import it again
            Log.Information("Check if file with exact this data already imported once...");
            var (fileInArchive, hashString) = await FileInArchiveCheck(file);
            if (!fileInArchive)
            {
                Log.Information("New file or data. Proceeding with import.");
                string[] entrySubparts;
                string[] stringArray = File.ReadAllLines(file);
                // Get the line with the market values
                string regularMvLine = stringArray[2];
                // Split the line into subparts
                string[] seperators = ["},{", "{{", "}}"];
                string[] regularMvSubparts = regularMvLine.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                // Get the timestamp from the line
                int subStart = regularMvLine.IndexOf("{downloadTime=", StringComparison.InvariantCulture) + "{downloadTime=".Length;
                int subEnd = regularMvLine.LastIndexOf(",fields", StringComparison.InvariantCulture);
                // Convert unix timestamp to DateTime and create column name for the timestamped market values
                DateTime dateTime =  new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                double luaUnixTimestamp = Convert.ToDouble(regularMvLine.Substring(subStart, subEnd - subStart), CultureInfo.InvariantCulture);
                string columnNameTimestamped = "date"+dateTime.AddSeconds(luaUnixTimestamp).ToLocalTime().ToString(CultureInfo.InvariantCulture).Replace("/", "_").Replace(" ", "time").Replace(":", "_");
                // Queries:
                // Add colum to table with timestamp
                var alterTable = @$"
                                ALTER TABLE regularMarketValues ADD COLUMN {columnNameTimestamped} REAL;";
                // Insert new itemId if not already in table
                var itemIdQuery = @"INSERT INTO 
                                regularMarketValues ( itemId )
                                VALUES ( @itemId )
                                ON CONFLICT(itemId) DO NOTHING;
                                SELECT itemId FROM regularMarketValues WHERE itemId = @itemId";
                // Insert market values into new column to table
                var marketValuesQuery = @$"UPDATE regularMarketValues
                                SET ( {columnNameTimestamped} ) = ( @{columnNameTimestamped} )
                                WHERE
                                itemId = @itemId;";

                await ExecuteWithRetryAsync(async () =>
                {
                    await using var transaction = await connection.BeginTransactionAsync();
                    // Try to execute the transaction with chance of retrying
                    try
                    {
                        await using (var sqlcmd = new SqliteCommand(alterTable, connection, (SqliteTransaction)transaction))
                        {
                            await sqlcmd.ExecuteNonQueryAsync();
                        }
                        foreach (var entry in regularMvSubparts)
                        {
                            if (entry == regularMvSubparts[0] || entry == regularMvSubparts[^1]) continue;
                            entrySubparts = entry.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            await using (var sqlcmd = new SqliteCommand(itemIdQuery, connection, (SqliteTransaction)transaction))
                            {
                                sqlcmd.Parameters.AddWithValue("@itemId", entrySubparts[0]);
                                await sqlcmd.ExecuteNonQueryAsync();
                            }
                            await using (var sqlcmd = new SqliteCommand(marketValuesQuery, connection, (SqliteTransaction)transaction))
                            {
                                sqlcmd.Parameters.AddWithValue("@itemId", entrySubparts[0]);
                                decimal mvNumbersDecimal = GFG.ToDeci(entrySubparts[1], 32);
                                sqlcmd.Parameters.AddWithValue($"@{columnNameTimestamped}", mvNumbersDecimal / 10000);
                                await sqlcmd.ExecuteNonQueryAsync();
                            }
                        }
                        // Commit
                        await transaction.CommitAsync();
                        Log.Information("Successfully Imported data from: " + file);
                    }
                    catch (Exception ex)
                    {
                        // Rollback on error
                        await transaction.RollbackAsync();
                        Log.Error("Error during lua data import transaction: " + ex);
                        ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->ProcessLuaFileAsync->try->transaction", ex);
                    }
                });
                // Archive file, add hash to list, compress file
                try
                {
                    Log.Information("Copying file to archive folder: " + fileToArchive);
                    File.Copy(file, fileToArchive);
                    await Task.Run(() => CompressFile(file, compressedFilePath));
                    Log.Information("Done importing data. Adding hash to archived_lua_hashes.txt");
                    await File.AppendAllTextAsync(DatabaseMain.dbLuaArchivePath + @"\archived_lua_hashes.txt", hashString + "\n");
                }
                catch (Exception ex)
                {
                    Log.Error($"Error finalizing file import: {file}");
                    Log.Error("Exception: " + ex);
                    ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->ProcessLuaFileAsync->finally->try->copy to archive||Append hash", ex);
                }
            }
            Log.Information("Going over to next file.");
            semaphore.Release(); // Release the semaphore object, allowing the next thread to act
        }
        public static async Task OrderColumns(SqliteConnection connection)
        {
            // Query to get columns in table
            var pragmaQuery = "PRAGMA table_info(regularMarketValues);";
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string orderedColumns = "";
                await using (var sqlcmd = new SqliteCommand(pragmaQuery, connection, (SqliteTransaction)transaction))
                {
                    DbDataReader pragmaResult = await sqlcmd.ExecuteReaderAsync();
                    List<string> columns = [];
                    while (pragmaResult.Read())
                    {
                        Log.Information(pragmaResult.GetString(1));
                        if (pragmaResult.GetString(1).Contains('_'))
                        {
                            // Add columns containting timestamp to list
                            columns.Add(pragmaResult.GetString(1));
                        }
                    }
                    // Order the columns
                    columns = columns.OrderBy(x => x).ToList();
                    foreach (var column in columns)
                    {
                        // Make subpart for query string with ordered columns
                        orderedColumns += "," + column + " REAL NULL";
                    }
                }
                // Query, creates ordered table, insert data, drop old table, rename new table
                var reorderedTableQuery = $@"CREATE TABLE [regularMarketValuesNew] (
                                            [itemId] bigint NOT NULL
                                            {orderedColumns}
                                            , CONSTRAINT [sqlite_master_PK_regularMarketValuesNew] PRIMARY KEY ([itemId])
                                            );
                                            CREATE INDEX [regularMarketValuesNew_idx_regularMarketValuesNew_itemId] ON [regularMarketValuesNew] ([itemId] ASC);
                                            CREATE UNIQUE INDEX [regularMarketValuesNew_sqlite_autoindex_regularMarketValuesNew_1] ON [regularMarketValuesNew] ([itemId] ASC);
                                            INSERT INTO [regularMarketValuesNew] (
                                            itemId
                                            {orderedColumns.Replace(" REAL NULL", "")}
                                            )
                                            SELECT itemId {orderedColumns.Replace(" REAL NULL", "")}
                                            FROM [regularMarketValues];
                                            DROP TABLE [regularMarketValues];
                                            ALTER TABLE [regularMarketValuesNew] RENAME TO [regularMarketValues];";
                await using (var sqlcmd = new SqliteCommand(reorderedTableQuery, connection, (SqliteTransaction)transaction))
                {
                    await sqlcmd.ExecuteNonQueryAsync();
                }
                // Commit
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync();
                Log.Error("Error during order market values columns transaction: " + ex);
                ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->OrderColumns->try->transaction", ex);
            }
        }
        // Check if hash of file is already stored
        public static async Task<Tuple<bool, string>> FileInArchiveCheck(string fileToCheck)
        {
            string[] storedHashes = await File.ReadAllLinesAsync(DatabaseMain.dbLuaArchivePath + @"\archived_lua_hashes.txt");
            string hashString = BitConverter.ToString(SHA256.HashData(File.OpenRead(fileToCheck)));
            Console.WriteLine(hashString);
            if (storedHashes.Contains(hashString))
            {
                Log.Information("File 1 to 1 already archived: " + fileToCheck);
                Log.Information("Skipping File.");
                return new Tuple<bool, string>(true , hashString);
            }
            else
            {
                return new Tuple<bool, string>(false, hashString);
            }
        }
        // Compress method
        private static async Task CompressFile(string fileToCompress, string compressedFile)
        {
            Log.Information("Compressing the file...");
            await using FileStream originalFileStream = File.OpenRead(fileToCompress);
            await using FileStream compressedFileStream = File.Create(compressedFile + ".compr");
            await using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
            await originalFileStream.CopyToAsync(compressor);
            Log.Information("File compressed.");
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
                    ExceptionHandling.ExceptionHandler("DataBaseImportCsvs.cs->ExecuteWithRetryAsync", ex);
                }
            }
            Log.Error("Failed to complete operation after multiple retries due to deadlocks.");
        }
    }
}