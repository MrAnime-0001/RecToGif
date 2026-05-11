using RecToGif.Models;

namespace RecToGif.Services
{
    public interface ISettingsService
    {
        void EnsureDirectoriesExist();
        string CreateNewTempSessionFolder();
        AppSettings LoadSettings();
        void SaveSettings(AppSettings settings);
        ShortcutMap LoadShortcuts();
        void SaveShortcuts(ShortcutMap map);
    }
}
