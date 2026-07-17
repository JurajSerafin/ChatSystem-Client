using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Session;

namespace ChatSystem.Client.Infrastructure.Session {
    internal sealed class SessionContext : ISessionContext {
        public UserId? CurrentUserId { get; private set; }
        public string? SessionToken { get; private set; }
        public bool IsAuthenticated => CurrentUserId.HasValue && !string.IsNullOrEmpty(SessionToken);

        public void SetSession(UserId userId, string sessionToken) {
            CurrentUserId = userId;
            SessionToken = sessionToken;
        }

        public void Clear() {
            CurrentUserId = null;
            SessionToken = null;
        }
    }
}
