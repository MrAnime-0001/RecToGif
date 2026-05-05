namespace RecToGif
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            var ctx = new ApplicationContext();
            new Forms.RecorderForm().Show();
            Application.Run(ctx);
        }
    }
}
