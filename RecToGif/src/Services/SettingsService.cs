using System;
using System.IO;
using System.Text.Json;
using RecToGif.Models;

namespace RecToGif.Services
{
    public class SettingsService
    {
        public static string BaseAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RecToGif");
        public static string TempFolderPath => Path.Combine(BaseAppDataPath, "Temp");
        public static string SettingsFilePath => Path.Combine(BaseAppDataPath, "settings.json");
        public static string ShortcutMapFilePath => Path.Combine(BaseAppDataPath, "shortcuts.json");

        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(BaseAppDataPath)) Directory.CreateDirectory(BaseAppDataPath);
            if (!Directory.Exists(TempFolderPath)) Directory.CreateDirectory(TempFolderPath);
        }

        public static string CreateNewTempSessionFolder()
        {
            string sessionPath = Path.Combine(TempFolderPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(sessionPath);
            return sessionPath;
        }

        public static AppSettings LoadSettings() => AppSettings.Load(SettingsFilePath);
        public static void SaveSettings(AppSettings settings) => settings.Save(SettingsFilePath);

        public static ShortcutMap LoadShortcuts()
        {
            if (!File.Exists(ShortcutMapFilePath)) return ShortcutMap.GetDefault();
            var json = File.ReadAllText(ShortcutMapFilePath);
            return JsonSerializer.Deserialize<ShortcutMap>(json) ?? ShortcutMap.GetDefault();
        }

        public static void SaveShortcuts(ShortcutMap map)
        {
            var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ShortcutMapFilePath, json);
        }
    }
}
