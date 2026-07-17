using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Session {
    /// <summary>
    /// Holds the currently active session state in memory.
    /// Populated on login/register, cleared on logout.
    /// </summary>
    internal interface ISessionContext {
        UserId? CurrentUserId { get; }
        string? SessionToken { get; }
        bool IsAuthenticated { get; }

        void SetSession(UserId userId, string sessionToken);
        void Clear();
    }
}
