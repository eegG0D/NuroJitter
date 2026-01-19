using System.Windows;

namespace NeuroJitter
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Catch startup errors
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Error: {args.Exception.Message}", "Crash Detected");
                args.Handled = true;
            };
            base.OnStartup(e);
        }
    }
}