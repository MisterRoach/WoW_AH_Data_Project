using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using WoWAHDataProject.Code;
using WoWAHDataProject.GUI.DatabaseGUI.ImportToDatabaseGUI.Pages;

namespace WoWAHDataProject.Database;

internal static class DatabaseImportCsvs
{
    // Declaring how many threads can access the semaphore code part at once
    // ensuring that only one thread can access the database at a time to prevent issues
    private static readonly SemaphoreSlim semaphore = new(1, 1);

    // Delay between retries in case of deadlock in ms and max number of retries
    // used in ExecuteWithRetryAsync
    private const int DelayBetweenRetries = 500;
    private const int maxRetries = 5;

    private static int amountOfFiles;

    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
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
    public static async Task<bool> DatabaseImportCsvFiles(IEnumerable<string> csvFiles, string connString, ImportData importData)
    {
        bool exceptionOccured = false;
        csvFiles = [.. csvFiles.OrderByDescending(File.GetLastWriteTime)];
        amountOfFiles = csvFiles.Count();
        importData.ProgressionText = $"Processed files: {importData.ProgressValue}/{amountOfFiles}";
        SqliteConnection connection = new(connString);
        IEnumerable<Task> tasks = csvFiles.Select(file => ProcessCsvFileAsync(file, connection, importData));
        try
        {
            await connection.OpenAsync();
            Log.Information("Opened database connection.");
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Log.Error("Error during processing csv files: " + ex);
            ExceptionHandling.ExceptionHandler("DataBaseImportCsvs.cs->DatabaseImportCsvFiles->try->WhenAll", ex);
            exceptionOccured = true;
        }
        finally
        {
            if (!exceptionOccured)
            {
                Log.Information("Processed all files.");
            }
            Log.Information("Closing database connection.");
            await connection.CloseAsync();
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        return exceptionOccured;
    }

    public static async Task ProcessCsvFileAsync(string file, SqliteConnection connection, ImportData importData)
    {
        string fileToArchive = Path.Combine(DatabaseMain.dbCsvArchivePath + @"\files", Path.GetFileNameWithoutExtension(file) + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture) + Path.GetExtension(file));
        string compressedFilePath = Path.Combine(DatabaseMain.dbCsvArchivePath + @"\files\compressed", Path.GetFileNameWithoutExtension(file) + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture));
        // Queue up threads trying to access the database
        await semaphore.WaitAsync();
        Log.Information("File to import: " + file);
        // Check if we got HashCode for the csv file already stored in archive, if not process it, if yes don't import it again
        Log.Information("Check if file with exact this data already imported once...");
        (bool fileInArchive, string hashString) = await FileInArchiveCheck(file);
        if (!fileInArchive)
        {
            Log.Information("New file or data. Proceeding with import.");
            string query = "";
            // Query snippet that gets append to the actual query depending on the file
            string insertIntoFields = @" (
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
                query = @"INSERT OR IGNORE INTO 
                                purchases" + insertIntoFields;
            }
            else if (file.Contains("sales"))
            {
                query = @"INSERT OR IGNORE INTO 
                                sales" + insertIntoFields;
            }
            // Queries to insert new otherPlayerNames and playerNames into their respective tables
            string queryOtherPlayer = @"
                                INSERT INTO otherPlayer (otherPlayerName)
                                VALUES (@otherPlayerName)
                                ON CONFLICT(otherPlayerName) DO NOTHING;
                                SELECT otherPlayerId FROM otherPlayer WHERE otherPlayerName = @otherPlayerName;";
            string queryPlayer = @"
                                INSERT INTO player (playerName)
                                VALUES (@playerName)
                                ON CONFLICT(playerName) DO NOTHING;
                                SELECT playerId FROM player WHERE playerName = @playerName;";
            // Read csv file to list
            List<AllEntries> rows = ReadEntriesFromCSV(file).ToList();
            List<string> playerNames = rows.Select(r => r.Player).Distinct().ToList();
            
            await ExecuteWithRetryAsync(async () =>
            {
                await using System.Data.Common.DbTransaction transaction = await connection.BeginTransactionAsync();
                // Try to execute the transaction with chance of retrying
                try
                {
                    foreach (AllEntries row in rows)
                    {
                        // Insert or get otherPlayerId and add (if not already) otherPlayerName to otherPlayer table
                        int otherPlayerId;
                        int playerId;
                        await using (SqliteCommand sqlcmd = new(queryOtherPlayer, connection, (SqliteTransaction)transaction))
                        {
                            sqlcmd.Parameters.AddWithValue("@otherPlayerName", row.OtherPlayer);
                            otherPlayerId = Convert.ToInt32(await sqlcmd.ExecuteScalarAsync(), CultureInfo.CurrentCulture);
                        }
                        // Same for the player
                        await using (SqliteCommand sqlcmd = new(queryPlayer, connection, (SqliteTransaction)transaction))
                        {
                            sqlcmd.Parameters.AddWithValue("@playerName", row.Player);
                            playerId = Convert.ToInt32(await sqlcmd.ExecuteScalarAsync(), CultureInfo.CurrentCulture);
                        }

                        // Insert new sales/purchases data into their respective tables
                        await using (SqliteCommand sqlcmd = new(query, connection, (SqliteTransaction)transaction))
                        {
                            sqlcmd.Parameters.AddWithValue("@itemString", row.ItemString);
                            sqlcmd.Parameters.AddWithValue("@itemName", row.ItemName);
                            sqlcmd.Parameters.AddWithValue("@stackSize", row.StackSize);
                            sqlcmd.Parameters.AddWithValue("@quantity", row.Quantity);
                            sqlcmd.Parameters.AddWithValue("@price", row.Price / 10000 * row.Quantity);
                            sqlcmd.Parameters.AddWithValue("@otherPlayerId", otherPlayerId);
                            sqlcmd.Parameters.AddWithValue("@playerId", playerId);
                            sqlcmd.Parameters.AddWithValue("@time", row.Time);
                            sqlcmd.Parameters.AddWithValue("@source", row.Source);
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
                    Log.Error("Error during csv import transaction: " + ex);
                    ExceptionHandling.ExceptionHandler("DataBaseImportCsvs.cs->ProcessCsvFileAsync->try->transaction", ex);
                }
            });
            // Archive file, add hash to list, compress file
            try
            {
                Log.Information("Copying file to archive folder: " + fileToArchive);
                File.Copy(file, fileToArchive);
                await Task.Run(() => CompressFile(file, compressedFilePath));
                Log.Information("Done importing data. Adding hash to archived_csv_hashes.txt");
                await File.AppendAllTextAsync(DatabaseMain.dbCsvArchivePath + @"\archived_csv_hashes.txt", hashString + "\n");
            }
            catch (Exception ex)
            {
                Log.Error($"Error finalizing file import: {file}");
                Log.Error("Exception: " + ex);
                ExceptionHandling.ExceptionHandler("DataBaseImportCsvs.cs->ProcessCsvFileAsync->finally->try->copy to archive||Append hash", ex);
            }
        }
        importData.ProgressValue++;
        Log.Information("Progress: " + importData.ProgressValue);
        Log.Information("Going over to next file.");
        // Release the semaphore object, allowing the next thread to act
        semaphore.Release();
    }
    // Check if hash of file is already stored
    public static async Task<Tuple<bool, string>> FileInArchiveCheck(string fileToCheck)
    {
        string[] storedHashes = await File.ReadAllLinesAsync(DatabaseMain.dbCsvArchivePath + @"\archived_csv_hashes.txt");
        string hashString = BitConverter.ToString(SHA256.HashData(File.OpenRead(fileToCheck)));
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
            catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6) // 5 = SQLITE_LOCKED, 6 = SQLITE_BUSY
            {
                retryCount++;
                Log.Warning($"Deadlock or busy exception detected. Retrying {retryCount}/{maxRetries}...");
                // Delay between retries, increasing with each retry
                await Task.Delay(DelayBetweenRetries * retryCount);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred: " + ex);
                ExceptionHandling.ExceptionHandler("DataBaseImportCsvs.cs->ExecuteWithRetryAsync", ex);
            }
        }
        Log.Error("Failed to complete operation after multiple retries due to deadlocks.");
    }
    // Method for reading csv entries
    public static IEnumerable<AllEntries> ReadEntriesFromCSV(string filePath)
    {
        using StreamReader reader = new(filePath);
        using CsvReader csv = new(reader, CsvConfig);

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
        string ItemString,
        string ItemName,
        int StackSize,
        int Quantity,
        decimal Price,
        string OtherPlayer,
        string Player,
        string Time,
        string Source
    );
}