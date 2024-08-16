using System.Data.Common;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Serilog;
using WoWAHDataProject.Code;
using WoWAHDataProject.GUI.DatabaseGUI.ImportToDatabaseGUI.Pages;

namespace WoWAHDataProject.Database;

public static partial class DatabaseImportMarketValues
{
    // Declaring how many threads can access the semaphore code part at once
    // ensuring only one file is processed at a time
    private static readonly SemaphoreSlim fileProcessingSemaphore = new(1, 1);

    // Delay between retries in case of deadlock in ms and max number of retries
    // used in ExecuteWithRetryAsync
    private const int DelayBetweenRetries = 500;

    private const int maxRetries = 5;

    private static int amountOfFiles;
    // Method started via button press in ImportMarketValuesToDatabase Window
    // Also using async, Task and await in the code to come so that UI doesn't freeze

    public static async Task<bool> DatabaseImportLuaMarketValues(IEnumerable<string> luaFiles, ImportData importData)
    {
        bool exceptionOccurred = false;
        amountOfFiles = luaFiles.Count();
        importData.ProgressionText = $"Processed files: {importData.ProgressValue}/{amountOfFiles}";

        try
        {
            IEnumerable<Task> tasks = luaFiles.Select(file => ProcessLuaFileAsync(file, importData));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Log.Error("Error during processing lua files: " + ex);
            ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->DatabaseImportMarketValues->try->WhenAll", ex);
            exceptionOccurred = true;
        }
        finally
        {
            Log.Information("Processed all files.");
        }
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        return exceptionOccurred;
    }

    public static async Task ProcessLuaFileAsync(string file, ImportData importData)
    {
        await fileProcessingSemaphore.WaitAsync();
        try
        {
            string fileToArchive = Path.Combine(DatabaseMain.dbLuaArchivePath + @"\files", Path.GetFileNameWithoutExtension(file) + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture) + Path.GetExtension(file));
            string compressedFilePath = Path.Combine(DatabaseMain.dbLuaArchivePath + @"\files\compressed", Path.GetFileNameWithoutExtension(file) + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture));
            // Queue up threads trying to access the database
            //await semaphore.WaitAsync();
            Log.Information("File to import: " + file);
            // Check if we got HashCode for the lua file already stored in archive, if not process it, if yes don't import it again
            Log.Information("Check if file with exact this data already imported once...");
            (bool fileInArchive, string hashString) = await FileInArchiveCheck(file);
            if (!fileInArchive)
            {
                Log.Information("New file or data. Proceeding with import.");
                string[] luaLines = File.ReadAllLines(file);

                await Parallel.ForEachAsync(luaLines, async (line, _) =>
                {
                    using SqliteConnection connection = new(DatabaseMain.connString);
                    await connection.OpenAsync(_);
                    if (line.Contains("\"marketValue\"") && (line.Contains("Horde") || line.Contains("\"marketValue\"") && line.Contains("Alliance")))
                    {
                        string realm = line.Split(["\",\"", "\",[["], StringSplitOptions.RemoveEmptyEntries)[1].Replace("-", "_");
                        Log.Information($"Processing import for {realm} MarketValues.");
                        bool doReorderColumns = await ProcessLineAsync(line, realm, realm + "_MarketValues", connection, file);
                        if (doReorderColumns)
                        {
                            await OrderColumns(connection, realm + "_MarketValues");
                        }
                    }
                    else if (line.Contains("\"regionMarketValue\""))
                    {
                        string region = line.Split(["\",\"", "\",[["], StringSplitOptions.RemoveEmptyEntries)[1].Replace("-", "_");
                        Log.Information($"Processing import for {region} RegionalMarketValues.");
                        bool doReorderColumns = await ProcessLineAsync(line, region, region + "_RegionalMarketValues", connection, file);
                        if (doReorderColumns)
                        {
                            await OrderColumns(connection, region + "_RegionalMarketValues");
                        }
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
            importData.ProgressValue++;
            importData.ProgressionText = $"Processed files: {importData.ProgressValue}/{amountOfFiles}";
            Log.Information("File processed.");
        }
        finally
        {
            // Release the semaphore, allowing the next thread/file to act
            fileProcessingSemaphore.Release();
        }
    }

    private static async Task<bool> ProcessLineAsync(string luaLine, string realmOrRegion, string tableName, SqliteConnection connection, string file)
    {
        bool doReorderColumns = false;
        // Split the line into subparts
        string[] seperators = ["},{", "{{", "}}"];
        string[] luaLineSubparts = luaLine.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
        // Get the timestamp from the line
        int subStart = luaLine.IndexOf("{downloadTime=", StringComparison.InvariantCulture) + "{downloadTime=".Length;
        int subEnd = luaLine.LastIndexOf(",fields", StringComparison.InvariantCulture);
        // Convert unix timestamp to DateTime and create column name for the timestamped market values
        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        double luaUnixTimestamp = Convert.ToDouble(luaLine[subStart..subEnd], CultureInfo.InvariantCulture);
        //double luaUnixTimestamp = Convert.ToDouble(luaLine.Substring(subStart, subEnd - subStart), CultureInfo.InvariantCulture);
        string columnNameTimestamped = "date" + dateTime.AddSeconds(luaUnixTimestamp).ToLocalTime().ToString(CultureInfo.InvariantCulture).Replace("/", "_").Replace(" ", "time").Replace(":", "_");

        bool tableExists = DataBaseCreation.TableExists(connection, tableName);
        if (!tableExists)
        {
            string whatsMissing = realmOrRegion.Contains("Horde") || realmOrRegion.Contains("Alliance") ? "realm + faction" : "region";
            Log.Warning($"Table for {whatsMissing} : {realmOrRegion} does not exist. Trying to create table.");
            try
            {
                await using (SqliteCommand createTableCmd = new(@$"
                                                                CREATE TABLE IF NOT EXISTS
                                                                {tableName} (
                                                                itemId INTEGER PRIMARY KEY UNIQUE
                                                                );", connection))
                {
                    await createTableCmd.ExecuteNonQueryAsync();
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Error creating table {tableName}: {ex}");
                ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->ProcessLineAsync->try->createTableCmd", ex);
            }
        }

        string pragmaQuery = $"PRAGMA table_info({tableName});";
        await using SqliteCommand sqlcmd = new(pragmaQuery, connection);
        SqliteDataReader pragmaResult = await sqlcmd.ExecuteReaderAsync();
        while (pragmaResult.Read())
        {
            if (pragmaResult.GetString(1).Contains(columnNameTimestamped))
            {
                Log.Warning($"While importing file {realmOrRegion} data from file {file}: Column {columnNameTimestamped} already in table {tableName}");
                Log.Information("Skipping line.");
                return doReorderColumns;
            }
        }
        // Queries:
        // Add colum to table with timestamp
        string alterTable = @$"
                            ALTER TABLE {tableName} ADD COLUMN {columnNameTimestamped} REAL;";
        // Insert new itemId if not already in table
        string itemIdQuery = @$"INSERT INTO
                            {tableName} ( itemId )
                            VALUES ( @itemId )
                            ON CONFLICT(itemId) DO NOTHING;
                            SELECT itemId FROM {tableName} WHERE itemId = @itemId";
        // Insert market values into new column to table
        string marketValuesQuery = @$"UPDATE {tableName}
                            SET ( {columnNameTimestamped} ) = ( @{columnNameTimestamped} )
                            WHERE
                            itemId = @itemId;";

        await ExecuteWithRetryAsync(async () =>
        {
            await using DbTransaction transaction = await connection.BeginTransactionAsync();
            // Try to execute the transaction with chance of retrying
            try
            {
                await using (SqliteCommand sqlcmd = new(alterTable, connection, (SqliteTransaction)transaction))
                {
                    await sqlcmd.ExecuteNonQueryAsync();
                }
                // Arry to hold the subparts of the line
                string[] entrySubparts;
                foreach (string entry in luaLineSubparts)
                {
                    if (entry == luaLineSubparts[0] || entry == luaLineSubparts[^1])
                    {
                        continue;
                    }
                    entrySubparts = entry.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    await using (SqliteCommand sqlcmd = new(itemIdQuery, connection, (SqliteTransaction)transaction))
                    {
                        sqlcmd.Parameters.AddWithValue("@itemId", entrySubparts[0]);
                        await sqlcmd.ExecuteNonQueryAsync();
                    }
                    await using (SqliteCommand sqlcmd = new(marketValuesQuery, connection, (SqliteTransaction)transaction))
                    {
                        sqlcmd.Parameters.AddWithValue("@itemId", entrySubparts[0]);
                        decimal mvNumbersDecimal = GFG.ToDeci(entrySubparts[1], 32);
                        sqlcmd.Parameters.AddWithValue($"@{columnNameTimestamped}", mvNumbersDecimal / 10000);
                        await sqlcmd.ExecuteNonQueryAsync();
                        //Log.Information($"Added mv {mvNumbersDecimal / 10000} for {entrySubparts[0]} to {tableName}");
                    }
                }
                // Commit
                await transaction.CommitAsync();
                Log.Information($"Import to {tableName} committed.");
                doReorderColumns = true;
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync();
                Log.Error($"Error in import transaction for {tableName}, file{file}:\n" + ex);
                ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->ProcessLuaFileAsync->try->transaction", ex);
                doReorderColumns = false;
            }
        });
        Log.Information($"Successfully imported {tableName}.");
        return doReorderColumns;
    }

    public static async Task OrderColumns(SqliteConnection connection, string tableName)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            Log.Information($"Try to reorder columns for {tableName}.");
            // Query to get columns in table
            string pragmaTableInfoQuery = $"PRAGMA table_info({tableName});";
            string pragmaIndexListQuery = $"PRAGMA index_list({tableName});";
            string index = $"{tableName}New_idx_{tableName}{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}_itemId";
            string autoIndex = $"{tableName}New_sqlite_autoindex_{tableName}{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}_1";
            string dropIndexQuery = "";
            await using DbTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                string orderedColumns = "";
                await using (SqliteCommand sqlcmd = new(pragmaTableInfoQuery, connection, (SqliteTransaction)transaction))
                {
                    SqliteDataReader pragmaResult = await sqlcmd.ExecuteReaderAsync();
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
#pragma warning disable IDE0305 // Simplify collection initialization
                    columns = columns.OrderByDescending(x => x).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization
                    foreach (string column in columns)
                    {
                        // Make subpart for query string with ordered columns
                        orderedColumns += "," + column + " REAL NULL";
                    }
                }
                await using (SqliteCommand sqlcmd = new(pragmaIndexListQuery, connection, (SqliteTransaction)transaction))
                {
                    SqliteDataReader pragmaResult = await sqlcmd.ExecuteReaderAsync();
                    while (pragmaResult.Read())
                    {
                        Log.Information(pragmaResult.GetString(1));
                        if (pragmaResult.GetString(1) == index)
                        {
                            dropIndexQuery += $"DROP INDEX [{index}];";
                        }
                        else if (pragmaResult.GetString(1) == autoIndex)
                        {
                            dropIndexQuery += $"DROP INDEX [{autoIndex}];";
                        }
                    }
                }
                // Query, creates ordered table, insert data, drop old table, rename new table
                string reorderedTableQuery = $@"{dropIndexQuery}
                                                CREATE TABLE [{tableName}New] (
                                                [itemId] bigint NOT NULL
                                                {orderedColumns}
                                                , CONSTRAINT [sqlite_master_PK_{tableName}New] PRIMARY KEY ([itemId])
                                                );
                                                CREATE INDEX [{tableName}New_idx_{tableName}{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}_itemId] ON [{tableName}New] ([itemId] ASC);
                                                CREATE UNIQUE INDEX [{tableName}New_sqlite_autoindex_{tableName}{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}_1] ON [{tableName}New] ([itemId] ASC);
                                                INSERT INTO [{tableName}New] (
                                                itemId
                                                {orderedColumns.Replace(" REAL NULL", "")}
                                                )
                                                SELECT itemId {orderedColumns.Replace(" REAL NULL", "")}
                                                FROM [{tableName}];
                                                DROP TABLE [{tableName}];
                                                ALTER TABLE [{tableName}New] RENAME TO [{tableName}];";
                await using (SqliteCommand sqlcmd = new(reorderedTableQuery, connection, (SqliteTransaction)transaction))
                {
                    await sqlcmd.ExecuteNonQueryAsync();
                }
                // Commit
                await transaction.CommitAsync();
                Log.Information($"Column reorder for {tableName} commited.");
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync();
                Log.Error("Error during order market values columns transaction: " + ex);
                //ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->OrderColumns->try->transaction", ex);
            }
        });
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
            return new Tuple<bool, string>(true, hashString);
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
        await using GZipStream compressor = new(compressedFileStream, CompressionMode.Compress);
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
            catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6 || ex.Message.Contains("already exists")) // 5 = SQLITE_LOCKED, 6 = SQLITE_BUSY
            {
                retryCount++;
                if (ex.SqliteErrorCode is 5 or 6)
                {
                    Log.Warning($"Deadlock or busy exception detected. Retrying {retryCount}/{maxRetries}...");
                }
                else if (ex.Message.Contains("already exists"))
                {
                    Log.Warning($"Index of rordered table already existed. Retrying {retryCount}/{maxRetries} since there should be new data and we were just too fast...");
                    return;
                }
                // Delay between retries, increasing with each retry
                await Task.Delay(DelayBetweenRetries * retryCount);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred: " + ex);
                ExceptionHandling.ExceptionHandler("DataBaseImportMarketValues.cs->ExecuteWithRetryAsync", ex);
            }
        }
        Log.Error("Failed to complete operation after multiple retries due to deadlocks.");
    }
}