using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using PPDMStudio.Services;
using PPDMStudio.Services.PPDMStudio.Services;
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
    }
}

