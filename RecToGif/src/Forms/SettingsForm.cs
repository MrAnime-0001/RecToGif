using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RecToGif.Models;
using RecToGif.Services;

// Resolve TextBox ambiguity (WinForms TextBox vs potentially WinUI TextBox via implicit usings)
using WinFormsTextBox = System.Windows.Forms.TextBox;

namespace RecToGif.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ISettingsService _settingsService;
        private AppSettings _settings;
        private ShortcutMap _shortcuts;
        // Maps hotkey TextBox -> action name
        private readonly Dictionary<WinFormsTextBox, string> _hotkeyBoxes = new();
        // Saved on LoadShortcuts so Reset can restore defaults
        private ShortcutMap _originalShortcuts;

        public SettingsForm(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _settings = _settingsService.LoadSettings();
            _shortcuts = _settingsService.LoadShortcuts();
            _originalShortcuts = ShortcutMap.GetDefault(); // keep a copy of defaults for Reset

            InitializeComponent();
            BuildHotkeysTable();
            LoadSettingsIntoControls();
            LoadHotkeysIntoControls();
            SubscribeEvents();
        }

        private void BuildHotkeysTable()
        {
            // Clear any existing rows (e.g., on re-call)
            _hotkeysTable.Controls.Clear();
            _hotkeysTable.RowStyles.Clear();
            _hotkeyBoxes.Clear();

            // Collect all actions from ShortcutMap
            var rows = new List<(string action, string key)>();
            foreach (var kvp in _shortcuts.ActionToKey)
            {
                rows.Add((kvp.Key, kvp.Value));
            }

            _hotkeysTable.RowCount = rows.Count;
            _hotkeysTable.ColumnCount = 2;
            _hotkeysTable.ColumnStyles[0].Width = 160;
            _hotkeysTable.ColumnStyles[1].Width = 160;

            int rowIdx = 0;
            foreach (var (action, key) in rows)
            {
                // Action label
                var lbl = new Label
                {
                    Text = action,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(5, 0, 0, 0),
                    Dock = DockStyle.Fill
                };
                _hotkeysTable.Controls.Add(lbl, 0, rowIdx);

                // Hotkey TextBox — editable
                var tb = new WinFormsTextBox();
                tb.Text = key;
                tb.ReadOnly = true;
                tb.TextAlign = HorizontalAlignment.Center;
                tb.BackColor = Color.FromArgb(60, 60, 60);
                tb.ForeColor = Color.White;
                tb.Margin = new Padding(0);
                tb.Height = 22;

                // Hook up key capture
                tb.KeyDown += HotkeyTextBox_KeyDown;
                // Right-click to clear
                tb.MouseClick += (s, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                        tb.Text = string.Empty;
                };

                _hotkeyBoxes[tb] = action;
                _hotkeysTable.Controls.Add(tb, 1, rowIdx);

                // Set row height
                _hotkeysTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                rowIdx++;
            }

            // Resize table to fit rows
            _hotkeysTable.Height = rows.Count * 30 + 10;
        }

        private void HotkeyTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not WinFormsTextBox tb) return;

            // Escape clears the hotkey
            if (e.KeyCode == Keys.Escape)
            {
                tb.Text = string.Empty;
                e.SuppressKeyPress = true;
                return;
            }

            // Build key string
            string keyString = BuildKeyString(e.KeyData);
            tb.Text = keyString;
            e.SuppressKeyPress = true;
        }

        private static string BuildKeyString(Keys keyData)
        {
            // Strip modifier flags to get the base key
            Keys key = keyData & ~Keys.Modifiers;
            string s = key switch
            {
                Keys.Left =>   "Left",
                Keys.Right =>  "Right",
                Keys.Up =>     "Up",
                Keys.Down =>   "Down",
                Keys.Space =>  "Space",
                Keys.Enter =>  "Enter",
                Keys.Escape => "Escape",
                Keys.Back =>   "Back",
                Keys.Tab =>    "Tab",
                Keys.Delete => "Delete",
                Keys.Insert => "Insert",
                Keys.Home =>   "Home",
                Keys.End =>    "End",
                Keys.PageUp => "PageUp",
                Keys.PageDown => "PageDown",
                Keys.F1 => "F1", Keys.F2 => "F2", Keys.F3 => "F3",
                Keys.F4 => "F4", Keys.F5 => "F5", Keys.F6 => "F6",
                Keys.F7 => "F7", Keys.F8 => "F8", Keys.F9 => "F9",
                Keys.F10 => "F10", Keys.F11 => "F11", Keys.F12 => "F12",
                _ => char.ToUpper((char)key) >= 'A' && char.ToUpper((char)key) <= 'Z'
                    ? char.ToUpper((char)key).ToString()
                    : key.ToString()
            };

            var parts = new List<string>();
            if (keyData.HasFlag(Keys.Control)) parts.Add("Ctrl");
            if (keyData.HasFlag(Keys.Alt))     parts.Add("Alt");
            if (keyData.HasFlag(Keys.Shift))   parts.Add("Shift");
            parts.Add(s);

            return string.Join("+", parts);
        }

        private void LoadSettingsIntoControls()
        {
            _numFps.Value = _settings.DefaultFps;
            _chkCaptureCursor.Checked = _settings.CaptureCursor;
            _txtGifski.Text = _settings.GifskiPath;
            _txtFfmpeg.Text = _settings.FfmpegPath;
        }

        private void LoadHotkeysIntoControls()
        {
            foreach (var kvp in _hotkeyBoxes)
            {
                string action = kvp.Value;
                if (_shortcuts.ActionToKey.TryGetValue(action, out var key))
                {
                    kvp.Key.Text = key;
                }
            }
        }

        private void SubscribeEvents()
        {
            _numFps.ValueChanged += (s, e) => _settings.DefaultFps = (int)_numFps.Value;
            _chkCaptureCursor.CheckedChanged += (s, e) => _settings.CaptureCursor = _chkCaptureCursor.Checked;

            _btnBrowseGifski.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "gifski.exe|gifski.exe|All executables|*.exe", Title = "Locate gifski.exe" };
                if (ofd.ShowDialog() == DialogResult.OK) _txtGifski.Text = ofd.FileName;
            };
            _txtGifski.TextChanged += (s, e) => _settings.GifskiPath = _txtGifski.Text;

            _btnBrowseFfmpeg.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "ffmpeg.exe|ffmpeg.exe|All executables|*.exe", Title = "Locate ffmpeg.exe" };
                if (ofd.ShowDialog() == DialogResult.OK) _txtFfmpeg.Text = ofd.FileName;
            };
            _txtFfmpeg.TextChanged += (s, e) => _settings.FfmpegPath = _txtFfmpeg.Text;

            _btnSave.Click += (s, e) =>
            {
                // Collect hotkey changes back into _shortcuts
                foreach (var kvp in _hotkeyBoxes)
                {
                    string action = kvp.Value;
                    string key = kvp.Key.Text.Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        // Capture old key so we can clean up KeyToAction below
                        _shortcuts.ActionToKey.TryGetValue(action, out var oldKey);

                        // If this key is already bound to a different action, unbind the old one
                        if (_shortcuts.KeyToAction.TryGetValue(key, out var existing) && existing != action)
                            _shortcuts.ActionToKey.Remove(existing);

                        _shortcuts.ActionToKey[action] = key;
                        _shortcuts.KeyToAction[key] = action;

                        // Remove the action's old key from the reverse map so no orphaned entry remains
                        if (oldKey != null && oldKey != key && _shortcuts.KeyToAction.ContainsKey(oldKey))
                            _shortcuts.KeyToAction.Remove(oldKey);
                    }
                    else
                    {
                        // Clear: remove both directions
                        if (_shortcuts.ActionToKey.TryGetValue(action, out var oldKey))
                        {
                            _shortcuts.ActionToKey.Remove(action);
                            _shortcuts.KeyToAction.Remove(oldKey);
                        }
                    }
                }

                _settingsService.SaveSettings(_settings);
                _settingsService.SaveShortcuts(_shortcuts);
                this.Close();
            };

            _btnResetHotkeys.Click += (s, e) =>
            {
                _shortcuts = _originalShortcuts.DeepClone();
                BuildHotkeysTable();
                LoadHotkeysIntoControls();
            };
        }
    }
}