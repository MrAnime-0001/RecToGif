using System;
using System.Collections.Generic;
using RecToGif.Models;

namespace RecToGif.Recorder
{
    public interface IInputHook : IDisposable
    {
        event EventHandler<string>? ShortcutPressed;
        void Start();
        void Stop();
        void ClearEvents();
        List<InputEvent> FlushEvents(long startTimestampMs, long endTimestampMs);
    }
}
