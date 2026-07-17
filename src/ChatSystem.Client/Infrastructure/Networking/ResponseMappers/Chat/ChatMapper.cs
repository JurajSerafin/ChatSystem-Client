using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Interfaces.Networking.Chat.DTOs;

namespace ChatSystem.Client.Infrastructure.Networking.ResponseMappers.Chat {
    internal static class ChatMapper {
        public static CachedChat ToCachedChat(SingleChatResponse response) {
            return new CachedChat() {
                CachedAt = DateTimeOffset.UtcNow,
                CreatedAt = response.CreatedAt,
                IsDeleted = false,
                Id = response.Id,
                LastActivityAt = response.LastActivityAt,
                LastMessageId = response.LastMessageId,
                Name = response.Name,
            };
        }
    }
}
