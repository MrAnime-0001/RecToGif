using System;
using System.Drawing;
using System.Threading.Tasks;
using RecToGif.Models;

namespace RecToGif.Recorder
{
    public interface IFrameWriter
    {
        Task SaveFrameAsync(Bitmap bitmap, FrameMeta meta);
    }
}
