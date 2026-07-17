using System.Linq;

namespace ChatSystem.Client.Core.Domain.User {
    internal class RegularUserRole : IUserRole {
        public static UserActions[] PerformableActions = [
            UserActions.SendMessage,
            UserActions.DeleteAccount,
            UserActions.DeleteOwnMessage,
            UserActions.ModifyLogin,
            UserActions.ModifyPassword,
        ];
        public static string GetTypeString() {
            return "REGULAR_USER";
        }
        public static bool CanPerform(UserActions action) {
            return PerformableActions.Any(actionI => actionI == action);
        }
    }
}
