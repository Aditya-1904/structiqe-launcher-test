using System.Windows;
using System.Windows.Threading;

namespace structIQe_Application_Manager.Launcher
{
    public partial class App : Application
    {
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Unhandled exception:\n{e.Exception.GetType().Name}: {e.Exception.Message}\n\n" +
                $"{e.Exception.StackTrace}",
                "Launcher Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}