using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RecToGif.Models;

namespace RecToGif.Editor
{
    public class EditorModel
    {
        private List<FrameItem> _frames = new();
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        
        public ProjectSettings ProjectSettings { get; private set; } = new();

        public IReadOnlyList<FrameItem> Frames => _frames;

        public async Task LoadSessionAsync(string sessionPath)
        {
            _frames.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            ProjectSettings = new ProjectSettings();

            var pngFiles = Directory.GetFiles(sessionPath, "*.png")
                .OrderBy(f => f)
                .ToList();

            foreach (var pngPath in pngFiles)
            {
                string metaPath = Path.ChangeExtension(pngPath, ".meta");
                if (File.Exists(metaPath))
                {
                    var json = await File.ReadAllTextAsync(metaPath);
                    var meta = JsonSerializer.Deserialize<FrameMeta>(json);
                    if (meta != null)
                    {
                        _frames.Add(new FrameItem(pngPath, meta));
                    }
                }
            }
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute(_frames, ProjectSettings);
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo(_frames, ProjectSettings);
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute(_frames, ProjectSettings);
                _undoStack.Push(command);
            }
        }
    }

    public class ProjectSettings
    {
        public Rectangle CropRegion { get; set; } = Rectangle.Empty;
        public Size TargetSize { get; set; } = Size.Empty;
        public List<VisualOverlay> Overlays { get; set; } = new();
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderThickness { get; set; } = 0;
    }

    public enum OverlayType { Text, Watermark }

    public class VisualOverlay
    {
        public OverlayType Type { get; set; }
        public string Content { get; set; } = string.Empty; // Text or Image Path
        public Rectangle Bounds { get; set; }
        public Color Color { get; set; } = Color.White;
        public Font? Font { get; set; }
        public float Opacity { get; set; } = 1.0f;
    }

    public class FrameItem
    {
        public string ImagePath { get; }
        public FrameMeta Metadata { get; }

        public FrameItem(string imagePath, FrameMeta metadata)
        {
            ImagePath = imagePath;
            Metadata = metadata;
        }
    }

    public interface ICommand
    {
        void Execute(List<FrameItem> frames, ProjectSettings settings);
        void Undo(List<FrameItem> frames, ProjectSettings settings);
    }
}
