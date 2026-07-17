using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
