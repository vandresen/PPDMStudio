using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using PPDMStudio.Services;
using PPDMStudio.Services.PPDMStudio.Services;
using System.Diagnostics;
using System.Windows;

namespace PPDMStudio
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();

            // Your Dapper services
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IWellListService, WellListService>();
            services.AddSingleton<IWellService, WellService>();

            services.AddMudServices();

            var serviceProvider = services.BuildServiceProvider();

            // Force DatabaseService to initialize immediately
            serviceProvider.GetRequiredService<IDatabaseService>();

            // Assign services directly to the control
            blazorWebView.Services = serviceProvider;

            // Add root component
            blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(PPDMStudio.Components.App)
            });
        }

        private void HelpUserGuide_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/vandresen/PPDMStudio/blob/master/PPDMStudio/Docs/UserGuide.md",
                UseShellExecute = true
            });
        }

        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "PPDMStudio v1.0\n\nA PPDM well data editor.\n\nhttps://github.com/vandresen/PPDMStudio",
                "About PPDMStudio",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}

