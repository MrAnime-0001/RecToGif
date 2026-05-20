using System;
using System.IO;
using System.Text.Json;
using RecToGif.Models;

namespace RecToGif.Services
{
    public class SettingsService : ISettingsService
    {
        private static readonly object _saveLock = new();

        private readonly string _basePath;

        public string TempFolderPath => Path.Combine(_basePath, "Temp");
        public string SettingsFilePath => Path.Combine(_basePath, "settings.json");
        public string ShortcutMapFilePath => Path.Combine(_basePath, "shortcuts.json");

        public SettingsService()
        {
            _basePath = Path.Combine(AppContext.BaseDirectory, "data");
        }

        public void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
            if (!Directory.Exists(TempFolderPath)) Directory.CreateDirectory(TempFolderPath);

            CleanupOldSessions();
        }

        private void CleanupOldSessions()
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
                    if (name.Length == 36 && Guid.TryParse(name, out _))
                    {
                        var age = DateTime.Now - Directory.GetCreationTime(dir);
                        if (age > TimeSpan.FromHours(2))
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

        public string CreateNewTempSessionFolder()
        {
            string sessionPath = Path.Combine(TempFolderPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(sessionPath);
            return sessionPath;
        }

        public AppSettings LoadSettings()
        {
            lock (_saveLock)
            {
                return AppSettings.Load(SettingsFilePath);
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            lock (_saveLock)
            {
                settings.Save(SettingsFilePath);
            }
        }

        public ShortcutMap LoadShortcuts()
        {
            lock (_saveLock)
            {
                if (!File.Exists(ShortcutMapFilePath)) return ShortcutMap.GetDefault();
                var json = File.ReadAllText(ShortcutMapFilePath);
                return JsonSerializer.Deserialize<ShortcutMap>(json) ?? ShortcutMap.GetDefault();
            }
        }

        public void SaveShortcuts(ShortcutMap map)
        {
            lock (_saveLock)
            {
                var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ShortcutMapFilePath, json);
            }
        }
    }
}
