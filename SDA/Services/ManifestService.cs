using Newtonsoft.Json;
using SteamAuth;
using System.Text;

namespace SDA.Services
{
    public class ManifestService
    {
        private static Manifest? _manifest;
        private readonly IWebHostEnvironment _environment;
        private readonly string _manifestPath;

        public ManifestService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _manifestPath = Path.Combine(_environment.ContentRootPath, "maFiles", "manifest.json");
        }

        public static string GetExecutableDir()
        {
            return AppContext.BaseDirectory;
        }

        public Manifest GetManifest(bool forceLoad = false)
        {
            // Check if already statically loaded
            if (_manifest != null && !forceLoad)
            {
                return _manifest;
            }

            // Find config dir and manifest file
            string maDir = Path.Combine(_environment.ContentRootPath, "maFiles");
            string manifestFile = Path.Combine(maDir, "manifest.json");

            // If there's no config dir, create it
            if (!Directory.Exists(maDir))
            {
                _manifest = GenerateNewManifest(false);
                return _manifest;
            }

            // If there's no manifest, throw exception
            if (!File.Exists(manifestFile))
            {
                throw new ManifestParseException();
            }

            try
            {
                string manifestContents = File.ReadAllText(manifestFile);
                _manifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);

                if (_manifest.Encrypted && _manifest.Entries.Count == 0)
                {
                    _manifest.Encrypted = false;
                    _manifest.Save();
                }

                _manifest.RecomputeExistingEntries();

                return _manifest;
            }
            catch (Exception)
            {
                throw new ManifestParseException();
            }
        }

        public static Manifest GenerateNewManifest(bool scanDir = false)
        {
            Manifest newManifest = new Manifest();
            newManifest.Encrypted = false;
            newManifest.PeriodicCheckingInterval = 5;
            newManifest.PeriodicChecking = false;
            newManifest.AutoConfirmMarketTransactions = false;
            newManifest.AutoConfirmTrades = false;
            newManifest.CheckAllAccounts = false;
            newManifest.Entries = new List<ManifestEntry>();

            if (scanDir)
            {
                string maDir = Path.Combine(AppContext.BaseDirectory, "maFiles");
                if (Directory.Exists(maDir))
                {
                    string[] maFiles = Directory.GetFiles(maDir, "*.maFile");
                    foreach (string maFile in maFiles)
                    {
                        try
                        {
                            SteamGuardAccount account = JsonConvert.DeserializeObject<SteamGuardAccount>(File.ReadAllText(maFile));
                            if (account != null)
                            {
                                ManifestEntry entry = new ManifestEntry();
                                entry.Filename = Path.GetFileName(maFile);
                                entry.SteamID = account.Session.SteamID;
                                newManifest.Entries.Add(entry);
                            }
                        }
                        catch (Exception)
                        {
                            // Skip corrupted files
                        }
                    }
                }
            }

            newManifest.Save();
            return newManifest;
        }

        public bool SaveAccount(SteamGuardAccount account)
        {
            try
            {
                var manifest = GetManifest();
                return manifest.SaveAccount(account, false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveAccount(SteamGuardAccount account)
        {
            try
            {
                var manifest = GetManifest();
                return manifest.RemoveAccount(account);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public class Manifest
        {
            [JsonProperty("encrypted")]
            public bool Encrypted { get; set; }

            [JsonProperty("first_run")]
            public bool FirstRun { get; set; } = true;

            [JsonProperty("entries")]
            public List<ManifestEntry> Entries { get; set; } = new();

            [JsonProperty("periodic_checking")]
            public bool PeriodicChecking { get; set; } = false;

            [JsonProperty("periodic_checking_interval")]
            public int PeriodicCheckingInterval { get; set; } = 5;

            [JsonProperty("periodic_checking_checkall")]
            public bool CheckAllAccounts { get; set; } = false;

            [JsonProperty("auto_confirm_market_transactions")]
            public bool AutoConfirmMarketTransactions { get; set; } = false;

            [JsonProperty("auto_confirm_trades")]
            public bool AutoConfirmTrades { get; set; } = false;

            public SteamGuardAccount[] GetAllAccounts(string? passKey = null, int limit = -1)
            {
                List<SteamGuardAccount> accounts = new List<SteamGuardAccount>();
                string maDir = Path.Combine(AppContext.BaseDirectory, "maFiles");

                foreach (ManifestEntry entry in Entries)
                {
                    if (limit > 0 && accounts.Count >= limit) break;

                    string maFile = Path.Combine(maDir, entry.Filename);
                    if (!File.Exists(maFile)) continue;

                    try
                    {
                        string contents = File.ReadAllText(maFile);
                        SteamGuardAccount account = JsonConvert.DeserializeObject<SteamGuardAccount>(contents);

                        if (Encrypted)
                        {
                            if (passKey == null) continue;
                            try
                            {
                                account = FileEncryptor.Decrypt(contents, passKey);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }

                        accounts.Add(account);
                    }
                    catch (Exception)
                    {
                        // Skip corrupted files
                    }
                }

                return accounts.ToArray();
            }

            public bool Save()
            {
                try
                {
                    string maDir = Path.Combine(AppContext.BaseDirectory, "maFiles");
                    if (!Directory.Exists(maDir))
                    {
                        Directory.CreateDirectory(maDir);
                    }

                    string manifestFile = Path.Combine(maDir, "manifest.json");
                    string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                    File.WriteAllText(manifestFile, json);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public void RecomputeExistingEntries()
            {
                string maDir = Path.Combine(AppContext.BaseDirectory, "maFiles");
                if (!Directory.Exists(maDir)) return;

                // Remove entries that no longer exist
                Entries.RemoveAll(entry => !File.Exists(Path.Combine(maDir, entry.Filename)));

                // Add new entries
                string[] maFiles = Directory.GetFiles(maDir, "*.maFile");
                foreach (string maFile in maFiles)
                {
                    string filename = Path.GetFileName(maFile);
                    if (!Entries.Any(e => e.Filename == filename))
                    {
                        try
                        {
                            string contents = File.ReadAllText(maFile);
                            SteamGuardAccount account = JsonConvert.DeserializeObject<SteamGuardAccount>(contents);
                            if (account != null)
                            {
                                ManifestEntry entry = new ManifestEntry
                                {
                                    Filename = filename,
                                    SteamID = account.Session.SteamID
                                };
                                Entries.Add(entry);
                            }
                        }
                        catch (Exception)
                        {
                            // Skip corrupted files
                        }
                    }
                }
            }

            public bool SaveAccount(SteamGuardAccount account, bool encrypt, string? passKey = null)
            {
                try
                {
                    string maDir = Path.Combine(AppContext.BaseDirectory, "maFiles");
                    if (!Directory.Exists(maDir))
                    {
                        Directory.CreateDirectory(maDir);
                    }

                    string filename = $"{account.Session.SteamID}.maFile";
                    string filepath = Path.Combine(maDir, filename);

                    string json = JsonConvert.SerializeObject(account, Formatting.Indented);
                    if (encrypt)
                    {
                        if (passKey == null) return false;
                        json = FileEncryptor.Encrypt(json, passKey);
                    }

                    File.WriteAllText(filepath, json);

                    // Update manifest
                    var existingEntry = Entries.FirstOrDefault(e => e.SteamID == account.Session.SteamID);
                    if (existingEntry == null)
                    {
                        existingEntry = new ManifestEntry
                        {
                            Filename = filename,
                            SteamID = account.Session.SteamID
                        };
                        Entries.Add(existingEntry);
                    }
                    else
                    {
                        existingEntry.Filename = filename;
                    }

                    Save();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool RemoveAccount(SteamGuardAccount account, bool deleteMaFile = true)
            {
                try
                {
                    var entry = Entries.FirstOrDefault(e => e.SteamID == account.Session.SteamID);
                    if (entry == null) return false;

                    if (deleteMaFile)
                    {
                        string maDir = Path.Combine(AppContext.BaseDirectory, "maFiles");
                        string filepath = Path.Combine(maDir, entry.Filename);
                        if (File.Exists(filepath))
                        {
                            File.Delete(filepath);
                        }
                    }

                    Entries.Remove(entry);
                    Save();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool ChangeEncryptionKey(string oldKey, string newKey)
            {
                try
                {
                    if (!Encrypted) return false;

                    string maDir = Path.Combine(AppContext.BaseDirectory, "maFiles");
                    if (!Directory.Exists(maDir)) return false;

                    // Get all accounts with old key
                    var accounts = GetAllAccounts(oldKey);
                    if (accounts.Length == 0) return false;

                    // Re-encrypt all accounts with new key
                    foreach (var account in accounts)
                    {
                        SaveAccount(account, true, newKey);
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public class ManifestEntry
        {
            [JsonProperty("encryption_iv")]
            public string? EncryptionIV { get; set; }

            [JsonProperty("encryption_salt")]
            public string? EncryptionSalt { get; set; }

            [JsonProperty("filename")]
            public string Filename { get; set; } = string.Empty;

            [JsonProperty("steamid")]
            public ulong SteamID { get; set; }
        }
    }

    public class ManifestParseException : Exception { }
} 