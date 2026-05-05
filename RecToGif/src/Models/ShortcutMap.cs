using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RecToGif.Models
{
    public class ShortcutMap
    {
        public Dictionary<string, string> ActionToKey { get; set; } = new();

        public static ShortcutMap GetDefault()
        {
            return new ShortcutMap
            {
                ActionToKey = new Dictionary<string, string>
                {
                    { "OpenRecorder", "F8" },
                    { "StartRecording", "F9" },
                    { "PauseRecording", "F9" },
                    { "StopRecording", "F10" },
                    { "DiscardRecording", "F11" }
                }
            };
        }
    }
}
