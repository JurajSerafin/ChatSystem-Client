using ChatSystem.Client.Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
