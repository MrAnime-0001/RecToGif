namespace RecToGif
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            new Forms.RecorderForm().Show();
            Application.Run();
        }
    }
}
