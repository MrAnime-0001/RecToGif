using System;
using Microsoft.Extensions.DependencyInjection;
using RecToGif.Forms;
using RecToGif.Presenters;
using RecToGif.Recorder;
using RecToGif.Services;

namespace RecToGif
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();

            // Singletons (shared state)
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IInputHook, InputHook>();
            services.AddSingleton<IShortcutService, ShortcutService>();

            // Forms (resolved via DI)
            services.AddTransient<RecorderForm>();
            services.AddTransient<EditorForm>();
            services.AddTransient<SettingsForm>();

            // Presenters (resolved via DI)
            services.AddTransient<RecorderPresenter>();
            services.AddTransient<EditorPresenter>();

            // Services
            services.AddTransient<IExportPipeline, ExportPipeline>();

            ServiceProvider = services.BuildServiceProvider();

            var mainForm = ServiceProvider.GetRequiredService<RecorderForm>();
            mainForm.Show();
            Application.Run();
        }
    }
}
