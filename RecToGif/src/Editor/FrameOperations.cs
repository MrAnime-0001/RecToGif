using System;
using System.Collections.Generic;
using System.Linq;

namespace RecToGif.Editor
{
    public class DeleteFramesCommand : ICommand
    {
        private readonly List<int> _indicesToDelete;
        private List<(int Index, FrameItem Item)>? _deletedItems;
        private bool _executed = false;

        public DeleteFramesCommand(IEnumerable<int> indices)
        {
            // Sort descending so we delete from highest index first — prevents index shift during deletion
            _indicesToDelete = indices.OrderByDescending(i => i).ToList();
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _deletedItems = new List<(int Index, FrameItem Item)>();
            foreach (int index in _indicesToDelete)
            {
                if (index >= 0 && index < frames.Count)
                {
                    _deletedItems.Add((index, frames[index]));
                    frames.RemoveAt(index);
                }
            }
            // DO NOT reverse — store in descending order (highest index first)
            // On undo, insert in that same order so earlier insertions don't shift later target positions
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(DeleteFramesCommand)} before Execute.");
            if (_deletedItems == null) return;
            // Insert in stored descending order: highest index first, so later lower-index inserts are unaffected
            foreach (var (index, item) in _deletedItems)
            {
                frames.Insert(index, item);
            }
        }
    }

    public class DuplicateFramesCommand : ICommand
    {
        private readonly List<int> _indicesToDuplicate;
        private List<int>? _newIndices;
        private bool _executed = false;

        public DuplicateFramesCommand(IEnumerable<int> indices)
        {
            _indicesToDuplicate = indices.OrderBy(i => i).ToList();
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _newIndices = new List<int>();
            int offset = 0;
            foreach (int index in _indicesToDuplicate)
            {
                int targetIndex = index + offset + 1;
                var original = frames[index + offset];
                var duplicate = new FrameItem(original.ImagePath, original.Metadata);
                frames.Insert(targetIndex, duplicate);
                _newIndices.Add(targetIndex);
                offset++;
            }
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(DuplicateFramesCommand)} before Execute.");
            if (_newIndices == null) return;
            foreach (int index in _newIndices.OrderByDescending(i => i))
            {
                frames.RemoveAt(index);
            }
        }
    }

    public class MoveFramesCommand : ICommand
    {
        private readonly List<int> _indicesToMove;
        private readonly int _direction; // -1 for left, 1 for right
        private List<int>? _oldIndices;
        private List<int>? _newIndices;
        private bool _executed = false;

        public MoveFramesCommand(IEnumerable<int> indices, int direction)
        {
            _indicesToMove = indices.OrderBy(i => i).ToList();
            _direction = direction;
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _oldIndices = _indicesToMove.ToList();
            _newIndices = new List<int>();

            if (_direction == -1) // Move Left
            {
                if (_indicesToMove.First() == 0)
                {
                    _newIndices = _indicesToMove.ToList();
                    return;
                }
                foreach (int index in _indicesToMove)
                {
                    var item = frames[index];
                    frames.RemoveAt(index);
                    frames.Insert(index - 1, item);
                    _newIndices.Add(index - 1);
                }
            }
            else if (_direction == 1) // Move Right
            {
                if (_indicesToMove.Last() == frames.Count - 1) return;
                foreach (int index in _indicesToMove.AsEnumerable().Reverse())
                {
                    var item = frames[index];
                    frames.RemoveAt(index);
                    frames.Insert(index + 1, item);
                    _newIndices.Add(index + 1);
                }
                _newIndices.Reverse();
            }
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(MoveFramesCommand)} before Execute.");
            if (_newIndices == null || _oldIndices == null) return;
            
            var items = _newIndices.Select(i => frames[i]).ToList();
            foreach (int index in _newIndices.OrderByDescending(i => i))
            {
                frames.RemoveAt(index);
            }
            for (int i = 0; i < _oldIndices.Count; i++)
            {
                frames.Insert(_oldIndices[i], items[i]);
            }
        }
    }

    public class ChangeDelayCommand : ICommand
    {
        private readonly List<int> _indices;
        private readonly int _newDelay;
        private List<(int Index, int OldDelay)>? _oldDelays;
        private bool _executed = false;

        public ChangeDelayCommand(IEnumerable<int> indices, int newDelay)
        {
            _indices = indices.ToList();
            _newDelay = newDelay;
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _oldDelays = new List<(int, int)>();
            foreach (int index in _indices)
            {
                _oldDelays.Add((index, frames[index].Metadata.DelayMs));
                frames[index].Metadata.DelayMs = _newDelay;
            }
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(ChangeDelayCommand)} before Execute.");
            if (_oldDelays == null) return;
            foreach (var (index, oldDelay) in _oldDelays)
            {
                frames[index].Metadata.DelayMs = oldDelay;
            }
        }
    }

    public class KeepRangeCommand : ICommand
    {
        private readonly int _start;
        private readonly int _end;
        private List<(int Index, FrameItem Item)>? _removedBefore;
        private List<(int Index, FrameItem Item)>? _removedAfter;
        private bool _executed = false;

        public KeepRangeCommand(int start, int end)
        {
            _start = start;
            _end = end;
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _removedBefore = new List<(int, FrameItem)>();
            _removedAfter = new List<(int, FrameItem)>();

            for (int i = frames.Count - 1; i > _end; i--)
            {
                _removedAfter.Add((i, frames[i]));
                frames.RemoveAt(i);
            }
            _removedAfter.Reverse();

            for (int i = _start - 1; i >= 0; i--)
            {
                _removedBefore.Add((i, frames[i]));
                frames.RemoveAt(i);
            }
            _removedBefore.Reverse();
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(KeepRangeCommand)} before Execute.");
            if (_removedBefore == null || _removedAfter == null) return;

            foreach (var (index, item) in _removedBefore)
            {
                frames.Insert(index, item);
            }
            foreach (var (index, item) in _removedAfter)
            {
                frames.Insert(index, item);
            }
        }
    }

    public class CropCommand : ICommand
    {
        private readonly Rectangle _newCrop;
        private Rectangle _oldCrop;
        private bool _executed = false;

        public CropCommand(Rectangle newCrop)
        {
            _newCrop = newCrop;
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _oldCrop = settings.CropRegion;
            settings.CropRegion = _newCrop;
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(CropCommand)} before Execute.");
            settings.CropRegion = _oldCrop;
        }
    }

    public class ResizeCommand : ICommand
    {
        private readonly Size _newSize;
        private Size _oldSize;
        private bool _executed = false;

        public ResizeCommand(Size newSize)
        {
            _newSize = newSize;
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _oldSize = settings.TargetSize;
            settings.TargetSize = _newSize;
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(ResizeCommand)} before Execute.");
            settings.TargetSize = _oldSize;
        }
    }

    public class AddOverlayCommand : ICommand
    {
        private readonly VisualOverlay _overlay;
        private bool _executed = false;

        public AddOverlayCommand(VisualOverlay overlay)
        {
            _overlay = overlay;
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            settings.Overlays.Add(_overlay);
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(AddOverlayCommand)} before Execute.");
            settings.Overlays.Remove(_overlay);
        }
    }

    public class UpdateBorderCommand : ICommand
    {
        private readonly Color _newColor;
        private readonly int _newThickness;
        private Color _oldColor;
        private int _oldThickness;
        private bool _executed = false;

        public UpdateBorderCommand(Color color, int thickness)
        {
            _newColor = color;
            _newThickness = thickness;
        }

        public void Execute(List<FrameItem> frames, ProjectSettings settings)
        {
            _executed = true;
            _oldColor = settings.BorderColor;
            _oldThickness = settings.BorderThickness;
            settings.BorderColor = _newColor;
            settings.BorderThickness = _newThickness;
        }

        public void Undo(List<FrameItem> frames, ProjectSettings settings)
        {
            if (!_executed)
                throw new InvalidOperationException($"Cannot undo {nameof(UpdateBorderCommand)} before Execute.");
            settings.BorderColor = _oldColor;
            settings.BorderThickness = _oldThickness;
        }
    }
}
