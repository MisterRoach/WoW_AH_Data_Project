using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using Serilog;
using WoWAHDataProject.Database;
using Forms = System.Windows.Forms;

namespace WoWAHDataProject.Code;

internal static class ConfigurationHelper
{
    public static void InitConfigCheck()
    {
        NameValueCollection appSettings = ConfigurationManager.AppSettings;
        System.Configuration.Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;
        string[] allKeys = settings.AllKeys;
        string[] keysToSearch =[
                                "baseDirectory",
                                "dbDirectory",
                                "dbFilePath",
                                "notFirstLaunch",
                                "dbArchivePath",
                                "dbCsvArchivePath",
                                "dbLuaArchivePath"
                               ];
        Log.Information("Configfile: " + configFile.FilePath);
        // Check if config file has notFirstLaunch entry, check and set config keys if so
        try
        {
            if (!allKeys.Contains("notFirstLaunch"))
            {
                CheckConfigKeys(allKeys, keysToSearch, appSettings, settings, configFile, false);
            }
            else
            {
                // If backup exists and not first launch, compare current config with backup and check if BaseDirectory changed
                if (!File.Exists(configFile.FilePath + ".bak"))
                {
                    return;
                }

                if (CompareConfigs(configFile.FilePath, configFile.FilePath + ".bak") && AppDomain.CurrentDomain.BaseDirectory == settings["baseDirectory"].Value)
                {
                    Log.Information(AppDomain.CurrentDomain.BaseDirectory);
                    Log.Information("Current .config is the same as the backuped one and we are in the same BaseDirectory as stored in the config.");
                    Log.Information("Checking if database is available.");
                    if (!File.Exists(settings["dbFilePath"].Value))
                    {
                        Log.Information("Database not found.");
                        return;
                    }

                    Log.Information("Database found.");
                }
                else
                {
                    // If current config differs from backup or BaseDirectory changed, check if database is available
                    Log.Warning("Current config or BaseDirectory differs from the old one.");
                    Log.Information("Checking if database is available.");
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "db\\maindatabase.db"))
                    {
                        Log.Information("Database found, no crucial situation. Updating .config values.");
                        // Update config values if database is found
                        CheckConfigKeys(allKeys, keysToSearch, appSettings, settings, configFile, true);
                    }
                    else
                    {
                        Log.Warning("Database not found.");
                        Log.Information("Trying to find it via config and config.bak values.");
                        // Try to find database via location values in .config and .config.bak values
                        // offer to copy it if found, could fail depending on permissions
                        Tuple<string, string> findDatabase = TryToFindDatabase(settings, configFile);
                        if (findDatabase.Item2 == "true")
                        {
                            Log.Information("Database found in: " + findDatabase.Item1 + ".");
                            CopyDatabaseDialog(findDatabase.Item1);
                        }
                        else
                        {
                            // If no database was found, offer to create a new one
                            // could fail aswell depending on permissions
                            Log.Error("Database not found in stored possible locations.");
                            if (MessageBox.Show("The application was neither able to find a database in the current directory nor in the paths stored in the .config and .config.bak files? Do you wish to create a new one?", "Database not found", MessageBoxButtons.YesNo) != DialogResult.Yes)
                            {
                                return;
                            }

                            DataBaseCreation.CreateDatabase(AppDomain.CurrentDomain.BaseDirectory + "db\\maindatabase.db", AppDomain.CurrentDomain.BaseDirectory + "db", AppDomain.CurrentDomain.BaseDirectory + "db\\db_archive", AppDomain.CurrentDomain.BaseDirectory + "db\\csv_archive", AppDomain.CurrentDomain.BaseDirectory + "db\\lua_archive", "Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "db\\maindatabase.db");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error in ConfigurationCheck.cs: " + ex.Message);
            ExceptionHandling.ExceptionHandler("ConfigurationCheck.cs->InitConfigCheck", ex);
        }
    }

    public static void CheckConfigKeys(
      string[] keyCollection,
      string[] keysToSearch,
      NameValueCollection appSettings,
      KeyValueConfigurationCollection keyValPairs,
      System.Configuration.Configuration configFile,
      bool overwrite)
    {
        // Check if wanted values are in config, also check if overwrite is needed, if so overwrite, else if not(possibly firstLaunch) add them
        foreach (string str in keysToSearch)
        {
            if (keyCollection.Contains<string>(str))
            {
                Log.Information("Key " + str + " found.");
                Log.Information("Value: " + appSettings[str]);
                if (overwrite)
                {
                    Log.Information("Overwrite set to true. Trying to set value for " + str + ".");
                    AddUptConfigKey(str, keyValPairs, configFile, true);
                }
            }
            else if (!keyCollection.Contains<string>(str))
            {
                Log.Information("Key " + str + " not found.");
                Log.Information("Trying to add key " + str + " to appSettings.");
                AddUptConfigKey(str, keyValPairs, configFile, false);
            }
            configFile.Save(ConfigurationSaveMode.Modified);
        }
    }

    public static void AddUptConfigKey(string key, KeyValueConfigurationCollection keyValPairs, Configuration configFile, bool overwrite)
    {
        // Add or overwrite values as needed
        string value = key switch
        {
            "baseDirectory" => AppDomain.CurrentDomain.BaseDirectory,
            "dbDirectory" => AppDomain.CurrentDomain.BaseDirectory + "db",
            "dbFilePath" => AppDomain.CurrentDomain.BaseDirectory + @"db\maindatabase.db",
            "dbArchivePath" => AppDomain.CurrentDomain.BaseDirectory + @"db\db_archive",
            "dbCsvArchivePath" => AppDomain.CurrentDomain.BaseDirectory + @"db\csv_archive",
            "dbLuaArchivePath" => AppDomain.CurrentDomain.BaseDirectory + @"db\lua_archive",
            "notFirstLaunch" => "yes",
            _ => throw new ArgumentException("Invalid key", nameof(key))
        };

        if (overwrite)
        {
            keyValPairs[key].Value = value;
            Log.Information($@"Updated key: {key}\nValue: {value}");
        }
        else
        {
            keyValPairs.Add(key, value);
            Log.Information($@"Added key: {key}\nValue: {value}");
        }
        SaveAndRefresh(configFile);
    }

    public static void SaveAndRefresh(System.Configuration.Configuration configFile)
    {
        configFile.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
        File.Copy(configFile.FilePath, configFile.FilePath + ".bak", true);
    }
    // Compare current config with backuped one
    private static bool CompareConfigs(string currentConfig, string bakConfig)
    {
        int curConfBytes;
        int bakConfBytes;
        FileStream curConfFs;
        FileStream bakConfFs;

        curConfFs = new FileStream(currentConfig, FileMode.Open);
        bakConfFs = new FileStream(bakConfig, FileMode.Open);

        if (curConfFs.Length != bakConfFs.Length)
        {
            curConfFs.Close();
            bakConfFs.Close();

            return false;
        }

        do
        {
            curConfBytes = curConfFs.ReadByte();
            bakConfBytes = bakConfFs.ReadByte();
        }
        while ((curConfBytes == bakConfBytes) && (curConfBytes != -1));

        curConfFs.Close();
        bakConfFs.Close();

        return curConfBytes == bakConfBytes;
    }

    private static void CopyDatabaseDialog(string pathToDatabase)
    {
        // Tell user we found database somewhere else, offer copy, could fail depending on permissions
        DialogResult oldDbPathResult = MessageBox.Show("Database was found in a different location, do you want to copy it?", "Database found in different location", MessageBoxButtons.YesNo);
        if (oldDbPathResult == Forms.DialogResult.OK)
        {
            try
            {
                File.Copy(pathToDatabase, AppDomain.CurrentDomain.BaseDirectory + @"db\", true);
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"db\maindatabase.db"))
                {
                    MessageBox.Show("Database copied successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error copying database: {ex.Message}");
                ExceptionHandling.ExceptionHandler("ConfigurationCheck.cs->CopyDatabaseDialog", ex);
            }
        }
    }

    private static Tuple<string, string> TryToFindDatabase(KeyValueConfigurationCollection keyValPairs, Configuration configFile)
    {
        // First look for database in regular .config dbFilePath value, if found, return result
        Log.Information("Looking for database in location stored in .config.");
        if (File.Exists(keyValPairs["dbFilePath"].Value))
        {
            Log.Information($"Found database in old dbFilePath location: {keyValPairs["dbFilePath"].Value}");
            return new Tuple<string, string>(keyValPairs["dbFilePath"].Value, "true");
        }
        Log.Information("Database not found in .config location.");
        var bakFLines = File.ReadAllLines(configFile.FilePath + ".bak");
        string oldDbFilePath = "";
        // If first look failed, check in .config.bak dbFilePath value
        Log.Information("Looking for database in location stored in .config.bak.");
        foreach (var line in bakFLines)
        {
            if (line.Contains("dbFilePath"))
            {
                var lineParts = line.Split('"');
                foreach (var part in lineParts)
                {
                    if (part.Contains("dbFilePath"))
                    {
                        oldDbFilePath = part;
                        Log.Information($"Found .config.bak dbFilePath: {oldDbFilePath}");
                    }
                }
            }
        }
        // Return information depending on result
        if (File.Exists(oldDbFilePath))
        {
            Log.Information("Database found in .config.bak location.");
            return new Tuple<string, string>(oldDbFilePath, "true");
        }
        else
        {
            Log.Error("Neither of the stored dbFilePaths lead to an existing database.");
            return new Tuple<string, string>("", "false");
        }
    }
}
