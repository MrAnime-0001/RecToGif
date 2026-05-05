# RecToGif

RecToGif is a high-performance, lightweight screen recording tool for Windows designed specifically for creating high-quality GIFs. It leverages modern Windows APIs for efficient capture and integrates professional-grade tools like `gifski` to ensure the best possible output quality.

## 🚀 Features

- **High-Performance Capture**: Utilizes the Windows Graphics Capture API (DirectX 11) for smooth, low-overhead recording.
- **Region Selection**: Record your entire screen or a specific window/region with ease.
- **Intelligent Loop Finder**: Automatically identifies the best start and end frames to create seamless, perfect loops.
- **Integrated Editor**: 
    - Reorder, duplicate, or delete frames.
    - Adjust per-frame delays.
    - Preview animations in real-time.
- **Input Visualization**: Optional overlay to display mouse clicks and keyboard shortcuts during recording.
- **Pro-Quality Export**: Powered by `gifski` for high-quality GIFs and supports `ffmpeg` for video exports.
- **Modern UI**: Clean, dark-themed Windows Forms interface optimized for .NET 10.

## 🛠️ Requirements

- **OS**: Windows 10 (Version 1903 or later) or Windows 11.
- **Runtime**: [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0).

## 📦 Installation

1. Download the latest release.
2. Extract the contents to a folder of your choice.
3. Ensure `gifski.exe` is present in the `tools/` directory.
4. Run `RecToGif.exe`.

## 📖 Usage

1. **Record**: Launch the recorder, select your target region, and hit the record button (or use the shortcut).
2. **Edit**: Once stopped, the editor opens automatically. Trim your recording, remove unwanted frames, or use the **Loop Finder** to find that perfect loop point.
3. **Export**: Choose your desired FPS and quality settings, then export as a GIF or MP4.

## ⚙️ Configuration

Settings can be customized via the `Settings` menu, including:
- Default FPS
- Capture cursor toggle
- Input overlay visibility
- Custom paths for `gifski` and `ffmpeg`

---

## 🏗️ Technical Stack

- **Language**: C# 13
- **Framework**: .NET 10 (WinForms)
- **Capture**: Windows.Graphics.Capture + SharpDX (DXGI)
- **Encoding**: gifski, ffmpeg

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
*Built with ❤️ for creators who need high-quality screen demos.*
