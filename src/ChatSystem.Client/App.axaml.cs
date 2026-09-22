using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChatSystem.Client.Config;
using ChatSystem.Client.Presentation.ViewModels;
using ChatSystem.Client.Presentation.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using ChatSystem.Client.Core.Interfaces.Presentation;

namespace ChatSystem.Client;

public partial class App : Application {

    private const string ConfigJsonFile = "appsettings.json";

    private IServiceProvider? _serviceProvider;

    public void SetServiceProvider(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);

        _serviceProvider = BuildConfiguredServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && _serviceProvider is not null
        ) {
            var mainWindowVm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = mainWindowVm };

            var navigation = _serviceProvider.GetRequiredService<INavigationService>();
            navigation.NavigateTo<LoginViewModel>();

            base.OnFrameworkInitializationCompleted();

            return;
        }

        throw new InvalidOperationException(
            """
            Application initialization failed: The application must run with a classic desktop lifetime
            and the dependency injection ServiceProvider must be initialized.
            """
        );
    }

    private static IServiceProvider BuildConfiguredServiceProvider() {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(path: ConfigJsonFile, optional: false, reloadOnChange: true)
            .Build();

        var serverOptions = config
            .GetSection(ServerOptions.SectionName)
            .Get<ServerOptions>() ?? new ServerOptions();

        var services = new ServiceCollection();

        ConfigureServices(services, serverOptions);

        var serviceProvider = services.BuildServiceProvider();

        if (Application.Current is App app) {
            app.SetServiceProvider(serviceProvider);
        }

        return serviceProvider;
    }
    private static void ConfigureServices(IServiceCollection services, ServerOptions serverOptions) {
        services.AddChatSystemNetworking(serverOptions);

        services.AddChatSystemServices();
    }
}