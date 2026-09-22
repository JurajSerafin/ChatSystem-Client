using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Identity;
using ChatSystem.Client.Core.Interfaces.Cryptography;
using ChatSystem.Client.Core.Interfaces.Database;
using ChatSystem.Client.Core.Interfaces.Ids;
using ChatSystem.Client.Core.Interfaces.Networking.Auth;
using ChatSystem.Client.Core.Interfaces.Networking.Auth.DTOs;
using ChatSystem.Client.Core.Interfaces.Repositories;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Interfaces.Presentation;

namespace ChatSystem.Client.Infrastructure.Services;

internal sealed class AuthService : IAuthService {
    private readonly IAuthApi _authApi;
    private readonly ISessionContext _sessionContext;
    private readonly IProfilePathProvider _profilePathProvider;
    private readonly ISessionScopeService _sessionScopeService;

    public AuthService(
        IAuthApi authApi,
        ISessionContext sessionContext,
        IProfilePathProvider profilePathProvider,
        ISessionScopeService sessionScopeService
    ) {
        _authApi = authApi;
        _sessionContext = sessionContext;
        _profilePathProvider = profilePathProvider;
        _sessionScopeService = sessionScopeService;
    }

    public async Task<CachedUser> RegisterAsync(string login, string password, CancellationToken cancellationToken = default) {

        _profilePathProvider.SetActiveProfile(login);

        var sessionProvider = _sessionScopeService.BeginSession();

        await sessionProvider.GetRequiredService<ChatSystemLocalDbContext>()
            .Database.EnsureCreatedAsync(cancellationToken);

        var keyManager = sessionProvider.GetRequiredService<IClientKeyManager>();

        var keyPair = await keyManager.GenerateAndProtectKeyPairAsync(password, cancellationToken);

        var response = await _authApi.RegisterAsync(new RegisterRequest(login, password, keyPair.PublicKey), cancellationToken);

        var identityRepository = sessionProvider.GetRequiredService<ILocalIdentityRepository>();

        var userRepository = sessionProvider.GetRequiredService<ILocalUserRepository>();

        var userId = IdFactory.Parse<UserId>(response.Id);

        _sessionContext.SetSession(userId, response.SessionToken);

        await identityRepository.StoreAsync(new CachedIdentity {
            Id = userId,
            SessionToken = response.SessionToken,
            Login = response.Login,
            Tag = response.Tag
        }, cancellationToken);

        var newUser = new CachedUser {
            CreatedAt = DateTimeOffset.UtcNow,
            Id = userId,
            Login = response.Login,
            PublicKey = keyPair.PublicKey,
            Role = RegularUserRole.GetTypeString(),
            Tag = response.Tag
        };

        await userRepository.UpsertAsync(newUser, cancellationToken);

        return newUser;
    }

    public async Task<CachedUser> LoginAsync(string login, string password, CancellationToken cancellationToken = default) {

        var response = await _authApi.LoginAsync(new LoginRequest(login, password), cancellationToken);

        _profilePathProvider.SetActiveProfile(login);

        var sessionProvider = _sessionScopeService.BeginSession();

        await sessionProvider.GetRequiredService<ChatSystemLocalDbContext>()
            .Database.EnsureCreatedAsync(cancellationToken);

        var keyManager = sessionProvider.GetRequiredService<IClientKeyManager>();
        var identityRepository = sessionProvider.GetRequiredService<ILocalIdentityRepository>();
        var userRepository = sessionProvider.GetRequiredService<ILocalUserRepository>();

        await keyManager.UnlockPrivateKeyAsync(password, cancellationToken);

        var publicKey = keyManager.GetPublicKey();

        var userId = IdFactory.Parse<UserId>(response.Id);

        _sessionContext.SetSession(userId, response.SessionToken);

        await identityRepository.StoreAsync(new CachedIdentity {
            Id = userId,
            SessionToken = response.SessionToken,
            Login = response.Login,
            Tag = response.Tag,
        }, cancellationToken);

        var user = new CachedUser {
            CreatedAt = DateTimeOffset.UtcNow,
            Id = userId,
            Login = response.Login,
            PublicKey = publicKey,
            Role = RegularUserRole.GetTypeString(),
            Tag = response.Tag,
        };

        await userRepository.UpsertAsync(user, cancellationToken);
        return user;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default) {
        if (!_sessionContext.IsAuthenticated) {
            return;
        }

        try {
            await _authApi.LogoutAsync(new LogoutRequest(_sessionContext.SessionToken!), cancellationToken);
        } catch (Refit.ApiException) { }

        var sessionProvider = _sessionScopeService.CurrScope;

        if (sessionProvider is not null) {
            var keyManager = sessionProvider.GetRequiredService<IClientKeyManager>();
            keyManager.LockPrivateKey();

            var identityRepository = sessionProvider.GetRequiredService<ILocalIdentityRepository>();
            await identityRepository.ClearAsync(cancellationToken);
        }

        _sessionContext.Clear();
        _sessionScopeService.EndSession();
    }
}