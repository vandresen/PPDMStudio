using System.Configuration;
using System.Data;
using System.Windows;

namespace PPDMStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += (sender, e) =>
            {
                var ex = e.Exception;
                var fullMessage = $"Message: {ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack: {ex.StackTrace}";
                MessageBox.Show(fullMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };
        }
    }

}
