using System;
using System.Collections.Generic;
using System.Linq;
using RecToGif.Models;
using RecToGif.Recorder;

namespace RecToGif.Services
{
    public class ShortcutService : IShortcutService
    {
        private readonly ShortcutMap _map;
        private readonly IInputHook _hook;

        public event EventHandler? OnStartRecording;
        public event EventHandler? OnStopRecording;
        public event EventHandler? OnPauseRecording;
        public event EventHandler? OnDiscardRecording;
        public event EventHandler? OnOpenRecorder;

        public ShortcutService(IInputHook hook, ISettingsService settingsService)
        {
            _hook = hook;
            _map = settingsService.LoadShortcuts();
            _hook.ShortcutPressed += HandleShortcut;
        }

        private void HandleShortcut(object? sender, string key)
        {
            if (!_map.KeyToAction.TryGetValue(key, out var action))
                return;

            switch (action)
            {
                case "StartRecording":
                    OnStartRecording?.Invoke(this, EventArgs.Empty);
                    break;
                case "StopRecording":
                    OnStopRecording?.Invoke(this, EventArgs.Empty);
                    break;
                case "PauseRecording":
                    OnPauseRecording?.Invoke(this, EventArgs.Empty);
                    break;
                case "DiscardRecording":
                    OnDiscardRecording?.Invoke(this, EventArgs.Empty);
                    break;
                case "OpenRecorder":
                    OnOpenRecorder?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
