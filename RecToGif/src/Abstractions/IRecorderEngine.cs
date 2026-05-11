using System;
using Windows.Graphics.Capture;

namespace RecToGif.Recorder
{
    public interface IRecorderEngine : IDisposable
    {
        int FrameCount { get; }
        void Start(GraphicsCaptureItem item);
        void Pause();
        void Resume();
        void Stop();
    }
}
