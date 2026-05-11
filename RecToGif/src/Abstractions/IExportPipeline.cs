using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RecToGif.Editor;

namespace RecToGif.Services
{
    public interface IExportPipeline
    {
        Task ExportAsync(string outputPath, string format, IReadOnlyList<FrameItem> frames, ProjectSettings settings, IProgress<int> progress, CancellationToken token);
    }
}
