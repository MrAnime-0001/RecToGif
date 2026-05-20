# RecToGif

RecToGif is a high-performance, lightweight screen recording tool for Windows designed specifically for creating high-quality GIFs. It leverages modern Windows APIs for efficient capture and integrates professional-grade tools like `gifski` to ensure the best possible output quality.

## Features

- **High-Performance Capture**: Utilizes the Windows Graphics Capture API (DirectX 11) for smooth, low-overhead recording.
- **Region Selection**: Record your entire screen or a specific window/region with ease.
- **Intelligent Loop Finder**: Automatically identifies the best start and end frames to create seamless, perfect loops.
- **Integrated Editor**:
    - Reorder, duplicate, or delete frames.
    - Adjust per-frame delays.
    - Preview animations in real-time.
- **Input Visualization**: Optional overlay to display mouse clicks and keyboard shortcuts during recording.
- **Pro-Quality Export**: Powered by `gifski` for high-quality GIFs; supports `ffmpeg` for MP4, WebM, and WebP exports.
- **Modern UI**: Clean, dark-themed Windows Forms interface optimized for .NET 10.

## Requirements

- **OS**: Windows 10 (Version 1903 or later) or Windows 11.
- **Runtime**: [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0).

## Installation

1. Download the latest release.
2. Extract the contents to a folder of your choice.
3. Ensure `gifski.exe` is present in the `tools/` directory (or configure its path in Settings).
4. Run `RecToGif.exe`.

## Usage

1. **Record**: Launch the recorder, select your target region, and hit the record button (or use the shortcut).
2. **Edit**: Once stopped, the editor opens automatically. Trim your recording, remove unwanted frames, or use the **Loop Finder** to find that perfect loop point.
3. **Export**: Choose your desired format (GIF, MP4, WebM, WebP), then export.

## Configuration

Settings can be customized via the Settings menu, including:
- Default FPS
- Capture cursor toggle
- Input overlay visibility
- Custom paths for `gifski` and `ffmpeg`

---

## Architecture

### Pattern

RecToGif uses a **WinForms MVP (Model-View-Presenter)** architecture with `Microsoft.Extensions.DependencyInjection` for the IoC container.

- **Views** (Forms) are passive — they implement interface contracts (`IRecorderView`, `IEditorView`) and forward user actions to presenters.
- **Presenters** hold all business logic; views hold no domain state.
- **Commands** implement `ICommand` for undo/redo, managed by `EditorModel`'s command stack.

### Layer Map

```
┌─────────────────────────────────────────────────────────────────┐
│                         Forms (Views)                           │
│  RecorderForm   EditorForm   SettingsForm   ExportProgressDialog│
└──────────────────────────┬──────────────────────────────────────┘
                           │ implements IRecorderView / IEditorView
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Presenters                               │
│   RecorderPresenter        EditorPresenter                      │
└──────┬───────────────────────────────────┬───────────────────────┘
       │                                   │
       ▼                                   ▼
┌──────────────────┐         ┌─────────────────────────────────────┐
│ Recorder         │         │ Editor                              │
│ src/Recorder/    │         │ src/Editor/                         │
│ RecorderEngine   │         │ EditorModel (command stack)         │
│ FrameWriter      │         │ FrameOperations (ICommand impls)    │
│ InputHook        │         │ LoopFinder                          │
│ CursorCapture    │         │                                     │
└──────┬───────────┘         └──────────────────┬──────────────────┘
       │                                        │
       ▼                                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Services                                 │
│   SettingsService    ShortcutService    ExportPipeline          │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  Models / Abstractions                                          │
│  src/Models/    src/Abstractions/                               │
└─────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility |
|-----------|----------------|
| `RecorderForm` | Passive recording UI — forwards all actions to `RecorderPresenter` |
| `EditorForm` | Passive editor UI — hosts FrameTimeline, PreviewPanel, LoopFinderPanel |
| `RecorderPresenter` | Recording lifecycle: region picker, start/pause/stop, shortcut wiring |
| `EditorPresenter` | Editor orchestration: commands, playback loop, export delegation |
| `RecorderEngine` | DirectX11 capture via Windows.Graphics.Capture; bitmap + cursor per frame |
| `FrameWriter` | Saves `{D5}.png` + `{D5}.meta` (JSON) to session temp directory |
| `InputHook` | Low-level Win32 hooks (keyboard + mouse) on dedicated STA thread; events buffered in `ConcurrentQueue` |
| `EditorModel` | Frame list + undo/redo command stack |
| `FrameOperations` | All `ICommand` implementations: Delete, Duplicate, Move, ChangeDelay, KeepRange, Crop, Resize, AddOverlay, UpdateBorder |
| `LoopFinder` | Grayscale MAE similarity search across frame pairs |
| `ExportPipeline` | Parallel frame rendering → gifski/ffmpeg subprocess → output file |
| `SettingsService` | JSON load/save of `AppSettings`; temp session directory cleanup |
| `FrameTimeline` | Virtual-scrolling thumbnail strip with async `ThumbnailCache` (50 MB LRU) |
| `PreviewPanel` | Scaled image display, crop handles, overlay rendering |
| `ThumbnailCache` | Memory-budget LRU async cache; bicubic resampling; clone-on-read |

### Key Data Flows

**Recording:**
1. User starts recording → `RecorderPresenter` → `RecorderEngine.Start(captureItem)`
2. `RecorderEngine` receives DirectX11 frames, crops to target region, converts to `Bitmap`
3. `FrameWriter` saves PNG + JSON meta to `%TEMP%\RecToGif_{guid}\`
4. `InputHook` collects keyboard/mouse events; flushed into each `FrameMeta` at capture time

**Editing:**
1. User action in `EditorForm` → `EditorPresenter` method call
2. Presenter constructs `ICommand`, calls `EditorModel.ExecuteCommand`
3. Command mutates the frame list; model pushes to undo stack
4. Presenter calls `DisplayFrames` / `ShowFramePreview` to refresh the view

**Export:**
1. User selects output path in `EditorForm` → `EditorPresenter.ExportAsync`
2. `ExportPipeline` renders all frames in parallel (`Parallel.ForAsync`, max 4 threads) applying crop/resize/overlays/border to a temp dir
3. gifski or ffmpeg subprocess consumes the rendered PNGs and writes the output file
4. Progress reported via `IProgress<int>`; cancellation via `CancellationToken`

### DI Registrations

| Lifetime | Registration |
|----------|-------------|
| Singleton | `ISettingsService` → `SettingsService` |
| Singleton | `IInputHook` → `InputHook` |
| Singleton | `IShortcutService` → `ShortcutService` |
| Transient | `AppSettings` (loaded from `ISettingsService`) |
| Transient | `IExportPipeline` → `ExportPipeline` |
| Transient | `RecorderForm`, `EditorForm`, `SettingsForm` |
| Transient | `RecorderPresenter`, `EditorPresenter` |

### Threading Model

- **UI thread (STA):** WinForms message loop; all form updates marshalled here via `InvokeIfRequired`
- **STA hook thread:** `InputHook` runs a dedicated `Thread(ApartmentState.STA)` — required by Win32 low-level hooks
- **Thread pool:** `RecorderEngine.OnFrameArrived` fires on a WinRT thread pool thread; frame saves dispatched as `Task.Run`
- **Parallel export:** `Parallel.ForAsync` with max 4 workers during frame rendering

---

## Technical Stack

- **Language**: C# 13
- **Framework**: .NET 10 (WinForms), target `net10.0-windows10.0.19041.0`
- **Capture**: `Windows.Graphics.Capture` + `SharpDX.Direct3D11` / `SharpDX.DXGI`
- **Encoding**: gifski (GIF), ffmpeg (MP4 / WebM / WebP)
- **DI**: `Microsoft.Extensions.DependencyInjection`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
*Built for creators who need high-quality screen demos.*
