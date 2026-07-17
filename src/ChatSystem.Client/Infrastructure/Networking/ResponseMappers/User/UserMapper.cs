using System;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Networking.User.DTOs;

namespace ChatSystem.Client.Infrastructure.Networking.ResponseMappers.User {
    internal class UserMapper {
        public static CachedUser ToCachedUser(SingleUserResponse response) {
            return new CachedUser {
                CreatedAt = DateTimeOffset.UtcNow,
                Tag = response.Tag,
                Login = response.Login,
                Id = response.Id,
                PublicKey = response.PublicKey,
                Role = response.Role
            };
        }
    }
}
