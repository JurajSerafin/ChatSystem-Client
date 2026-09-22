using ChatSystem.Client.Core.Interfaces.Cryptography;
using ChatSystem.Client.Core.Interfaces.Database;
using ChatSystem.Client.Core.Interfaces.Networking.Auth;
using ChatSystem.Client.Core.Interfaces.Networking.Chat;
using ChatSystem.Client.Core.Interfaces.Networking.Message;
using ChatSystem.Client.Core.Interfaces.Networking.User;
using ChatSystem.Client.Core.Interfaces.Presentation;
using ChatSystem.Client.Core.Interfaces.Repositories;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;
using ChatSystem.Client.Infrastructure.Cryptography;
using ChatSystem.Client.Infrastructure.Database;
using ChatSystem.Client.Infrastructure.Networking;
using ChatSystem.Client.Infrastructure.Networking.Json;
using ChatSystem.Client.Infrastructure.Presentation;
using ChatSystem.Client.Infrastructure.Repositories;
using ChatSystem.Client.Infrastructure.Services;
using ChatSystem.Client.Infrastructure.Session;
using ChatSystem.Client.Presentation.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Text.Json;

namespace ChatSystem.Client.Config;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddChatSystemServices(this IServiceCollection services) {

        services.AddSingleton<ISessionScopeService, SessionScopeService>();
        
        AddDbContext(services);

        services.AddCryptographyServices();
        services.AddChatSystemRepositories();
        services.AddChatSystemInfrastructureServices();

        services.AddChatSystemPresentationServices();

        return services;
    }

    private static void AddChatSystemRepositories(this IServiceCollection services) {
        services.AddScoped<ILocalIdentityRepository, LocalIdentityRepository>();
        services.AddScoped<ILocalChatRepository, LocalChatRepository>();
        services.AddScoped<ILocalMessageRepository, LocalMessageRepository>();
        services.AddScoped<ILocalUserRepository, LocalUserRepository>();
    }

    private static void AddChatSystemInfrastructureServices(this IServiceCollection services) {
        services.AddSingleton<ISessionContext, SessionContext>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IChatService, ChatService>();
    }

    private static void AddChatSystemPresentationServices(this IServiceCollection services) {
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISessionScopeService, SessionScopeService>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddScoped<ChatShellViewModel>();
        services.AddScoped<ChatListViewModel>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistrationViewModel>();

        // UserSearchViewModel and ChatViewModel are intentionally not registered;
        // ChatViewModel needs a runtime CachedChat parameter and
        // UserSearchViewModel must be freshly created every time search opens.
        // Both are built via ActivatorUtilities.CreateInstance against the current
        // session scope, from ChatShellViewModel.CurrentProvider.
    }

    private static void AddDbContext(IServiceCollection services) {
        services.AddSingleton<IProfilePathProvider, ProfilePathProvider>();

        services.AddDbContext<ChatSystemLocalDbContext>((sp, options) => {
            var pathProvider = sp.GetRequiredService<IProfilePathProvider>();

            var dbPath = pathProvider.GetDatabasePath();
            options.UseSqlite($"Data Source={dbPath}");
        });
    }

    public static IServiceCollection AddCryptographyServices(this IServiceCollection services) {
        services.AddSingleton<IClientEncryptionService, AesRsaEncryptionService>();
        services.AddSingleton<IKeyDerivationService, Argon2KeyDerivationService>();
        services.AddScoped<IKeyStore, EFKeyStore>();
        services.AddScoped<IClientKeyManager, ClientKeyManager>();

        return services;
    }

    public static IServiceCollection AddChatSystemNetworking(
        this IServiceCollection services,
        ServerOptions serverOptions
    ) {
        services.AddSingleton(serverOptions);

        services.AddTransient<AuthTokenHandler>();

        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new UnixSecondsDateTimeOffsetConverter());

        var refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(jsonOptions));

        // no auth handler needed requests don't have tokens yet
        services.AddRefitClient<IAuthApi>(refitSettings)
            .ConfigureHttpClient(client => ConfigureHttpClient(client, serverOptions));

        // Attach the token on every request for the created APIs
        services.AddRefitClient<IUserApi>(refitSettings)
            .ConfigureHttpClient(client => ConfigureHttpClient(client, serverOptions))
            .AddHttpMessageHandler<AuthTokenHandler>();

        services.AddRefitClient<IChatApi>(refitSettings)
            .ConfigureHttpClient(client => ConfigureHttpClient(client, serverOptions))
            .AddHttpMessageHandler<AuthTokenHandler>();

        services.AddRefitClient<IMessageApi>(refitSettings)
            .ConfigureHttpClient(client => ConfigureHttpClient(client, serverOptions))
            .AddHttpMessageHandler<AuthTokenHandler>();

        return services;
    }

    private static void ConfigureHttpClient(System.Net.Http.HttpClient client, ServerOptions options) {
        client.BaseAddress = new Uri(options.BaseUrl);

        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    }
}