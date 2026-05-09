using System;
using System.IO;
using System.Text.Json;
using RecToGif.Models;

namespace RecToGif.Services
{
    public class SettingsService
    {
        private static readonly object _saveLock = new();

        public static string BaseAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RecToGif");
        public static string TempFolderPath => Path.Combine(BaseAppDataPath, "Temp");
        public static string SettingsFilePath => Path.Combine(BaseAppDataPath, "settings.json");
        public static string ShortcutMapFilePath => Path.Combine(BaseAppDataPath, "shortcuts.json");

        /// <summary>
        /// Ensures the app data directory exists and removes any stale session folders
        /// left behind from previous runs. Safe to call on every startup.
        /// </summary>
        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(BaseAppDataPath)) Directory.CreateDirectory(BaseAppDataPath);
            if (!Directory.Exists(TempFolderPath)) Directory.CreateDirectory(TempFolderPath);

            // Clean up orphaned GUID-named session folders older than 24 hours
            CleanupOldSessions();
        }

        private static void CleanupOldSessions()
        {
            if (!Directory.Exists(TempFolderPath)) return;

            string[] subDirs;
            try { subDirs = Directory.GetDirectories(TempFolderPath); }
            catch { return; }

            foreach (var dir in subDirs)
            {
                try
                {
                    var name = Path.GetFileName(dir);
                    // GUID format: 36 chars with hyphens (e.g. a1b2c3d4-...)
                    if (name.Length == 36 && Guid.TryParse(name, out _))
                    {
                        var age = DateTime.Now - Directory.GetCreationTime(dir);
                        if (age > TimeSpan.FromHours(24))
                        {
                            Directory.Delete(dir, true);
                            System.Diagnostics.Debug.WriteLine($"[SettingsService] Cleaned up stale session: {name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SettingsService] Could not delete session folder: {ex.Message}");
                }
            }
        }

        public static string CreateNewTempSessionFolder()
        {
            string sessionPath = Path.Combine(TempFolderPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(sessionPath);
            return sessionPath;
        }

        public static AppSettings LoadSettings() => AppSettings.Load(SettingsFilePath);

        public static void SaveSettings(AppSettings settings)
        {
            lock (_saveLock)
            {
                settings.Save(SettingsFilePath);
            }
        }

        public static ShortcutMap LoadShortcuts()
        {
            if (!File.Exists(ShortcutMapFilePath)) return ShortcutMap.GetDefault();
            var json = File.ReadAllText(ShortcutMapFilePath);
            return JsonSerializer.Deserialize<ShortcutMap>(json) ?? ShortcutMap.GetDefault();
        }

        public static void SaveShortcuts(ShortcutMap map)
        {
            lock (_saveLock)
            {
                var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ShortcutMapFilePath, json);
            }
        }
    }
}