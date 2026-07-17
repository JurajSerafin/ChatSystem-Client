using System;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Identity;
using ChatSystem.Client.Core.Interfaces.Cryptography;
using ChatSystem.Client.Core.Interfaces.Ids;
using ChatSystem.Client.Core.Interfaces.Networking.Auth;
using ChatSystem.Client.Core.Interfaces.Networking.Auth.DTOs;
using ChatSystem.Client.Core.Interfaces.Networking.User;
using ChatSystem.Client.Core.Interfaces.Repositories;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;

namespace ChatSystem.Client.Infrastructure.Services {
    internal class AuthService : IAuthService {
        private readonly IAuthApi _authApi;

        private readonly IUserApi _userApi;

        private readonly IClientKeyManager _keyManager;

        private readonly ILocalIdentityRepository _identityRepository;

        private readonly ILocalUserRepository _userRepository;

        private readonly ISessionContext _sessionContext;

        public AuthService(IAuthApi authApi, IClientKeyManager keyManager, ILocalIdentityRepository identityRepository, ILocalUserRepository userRepository, ISessionContext sessionContext, IUserApi userApi) {
            _authApi = authApi;
            _keyManager = keyManager;
            _identityRepository = identityRepository;
            _userRepository = userRepository;
            _sessionContext = sessionContext;
            _userApi = userApi;
        }

        public async Task<CachedUser> RegisterAsync(string login, string password, CancellationToken cancellationToken = default) {
            var keyPair = await _keyManager.GenerateAndProtectKeyPairAsync(password, cancellationToken);

            var response = await _authApi.RegisterAsync(new RegisterRequest(
                login, password, keyPair.PublicKey), cancellationToken);

            var userId = IdFactory.Parse<UserId>(response.Id);
            _sessionContext.SetSession(userId, response.SessionToken);

            await _identityRepository.StoreAsync(new CachedIdentity {
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

            await _userRepository.UpsertAsync(newUser, cancellationToken);

            return newUser;
        }
        public async Task<CachedUser> LoginAsync(string login, string password, CancellationToken cancellationToken = default) {
            var response = await _authApi.LoginAsync(
                new LoginRequest(login, password),
                cancellationToken
            );

            await _keyManager.UnlockPrivateKeyAsync(password, cancellationToken);

            var userId = IdFactory.Parse<UserId>(response.Id);

            _sessionContext.SetSession(userId, response.SessionToken);

            await _identityRepository.StoreAsync(new CachedIdentity {
                Id = userId,
                SessionToken = response.SessionToken,
                Login = response.Login,
                Tag = response.Tag
            }, cancellationToken);

            var publicKeyResponse = await _userApi.GetPublicKeyAsync(userId, cancellationToken);

            var user = new CachedUser {
                CreatedAt = DateTimeOffset.UtcNow,
                Id = userId,
                Login = response.Login,
                PublicKey = publicKeyResponse.PublicKey,
                Role = RegularUserRole.GetTypeString(),
                Tag = response.Tag,
            };
            await _userRepository.UpsertAsync(user, cancellationToken);

            return user;
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default) {
            if (!_sessionContext.IsAuthenticated) {
                return;
            }

            try {
                await _authApi.LogoutAsync(
                    new LogoutRequest(_sessionContext.SessionToken!),
                    cancellationToken);
            } catch (Refit.ApiException) {}

            _keyManager.LockPrivateKey();

            _sessionContext.Clear();
            await _identityRepository.ClearAsync(cancellationToken);
        }
    }
}
