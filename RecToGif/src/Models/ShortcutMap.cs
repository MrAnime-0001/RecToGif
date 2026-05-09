using System;
using System.Collections.Generic;

namespace RecToGif.Models
{
    public class ShortcutMap
    {
        public Dictionary<string, string> ActionToKey { get; set; } = new();
        // Inverse map — rebuilt from ActionToKey to guarantee consistency
        public Dictionary<string, string> KeyToAction { get; set; } = new();

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
                },
                KeyToAction = new Dictionary<string, string>
                {
                    { "F8", "OpenRecorder" },
                    { "F9", "StartRecording" },
                    { "F10", "StopRecording" },
                    { "F11", "DiscardRecording" }
                }
            };
        }

        /// <summary>
        /// Sets a key for an action, automatically unbinding any previous owner of that key
        /// and removing the action's old key from the inverse map. Keeps both dicts in sync.
        /// </summary>
        public void AddRebindKey(string action, string key)
        {
            // Unbind the old key this action previously had (if any)
            if (ActionToKey.TryGetValue(action, out var oldKey) && !string.IsNullOrEmpty(oldKey))
            {
                KeyToAction.Remove(oldKey);
            }

            // If the key is already bound to a DIFFERENT action, unbind that action
            if (KeyToAction.TryGetValue(key, out var existingAction) && existingAction != action)
            {
                ActionToKey.Remove(existingAction);
            }

            ActionToKey[action] = key;
            KeyToAction[key] = action;
        }

        /// <summary>
        /// Clears the key for an action, removing it from both maps.
        /// </summary>
        public void UnbindAction(string action)
        {
            if (ActionToKey.TryGetValue(action, out var oldKey) && !string.IsNullOrEmpty(oldKey))
            {
                KeyToAction.Remove(oldKey);
            }
            ActionToKey.Remove(action);
        }

        /// <summary>
        /// Rebuilds the KeyToAction inverse map from ActionToKey, correcting any drift.
        /// </summary>
        public void RebuildInverse()
        {
            KeyToAction.Clear();
            foreach (var kvp in ActionToKey)
            {
                KeyToAction[kvp.Value] = kvp.Key;
            }
        }

        /// <summary>
        /// Returns a deep copy of this ShortcutMap.
        /// </summary>
        public ShortcutMap DeepClone()
        {
            return new ShortcutMap
            {
                ActionToKey = new Dictionary<string, string>(ActionToKey),
                KeyToAction = new Dictionary<string, string>(KeyToAction)
            };
        }
    }
}
