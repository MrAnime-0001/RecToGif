using System;

namespace RecToGif.Services
{
    public interface IShortcutService
    {
        event EventHandler? OnStartRecording;
        event EventHandler? OnStopRecording;
        event EventHandler? OnPauseRecording;
        event EventHandler? OnDiscardRecording;
        event EventHandler? OnOpenRecorder;
    }
}
