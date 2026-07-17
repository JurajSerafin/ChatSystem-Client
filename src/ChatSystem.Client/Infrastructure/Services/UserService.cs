using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Networking.User;
using ChatSystem.Client.Core.Interfaces.Repositories;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Infrastructure.Networking.ResponseMappers.User;

namespace ChatSystem.Client.Infrastructure.Services;

internal sealed class UserService : IUserService {
    private const int SearchPageLimit = 20;

    private readonly IUserApi _userApi;
    private readonly ILocalUserRepository _userRepo;

    public UserService(IUserApi userApi, ILocalUserRepository userRepo) {
        _userApi = userApi;
        _userRepo = userRepo;
    }

    public async Task<IReadOnlyList<CachedUser>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default) {

        var response = await _userApi.SearchUsersAsync(
            query, SearchPageLimit, 0, cancellationToken);

        var users = response.Select(UserMapper.ToCachedUser).ToList();

        foreach (var user in users) {
            await _userRepo.UpsertAsync(user, cancellationToken);
        }

        return users;
    }

    public async Task<CachedUser?> GetByIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var cached = await _userRepo.FindByIdAsync(userId, cancellationToken);

        if (cached is not null) {
            return cached;
        }

        try {
            var response = await _userApi.GetByIdAsync(userId, cancellationToken);

            var user = UserMapper.ToCachedUser(response);

            await _userRepo.UpsertAsync(user, cancellationToken);
            return user;
        } catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
            return null;
        }
    }

    public async Task<string> GetPublicKeyAsync(
        UserId userId,
        CancellationToken cancellationToken = default
    ) {
        var user = await GetByIdAsync(userId, cancellationToken);
        
        if (user is not null) {
            return user.PublicKey;
        }

        var response = await _userApi.GetPublicKeyAsync(userId, cancellationToken);
        return response.PublicKey;
    }
}