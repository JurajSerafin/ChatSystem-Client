using ChatSystem.Client.Core.Interfaces.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Client.Infrastructure.Database {
    internal class ProfilePathProvider : IProfilePathProvider {
        private string? _activeLogin;

        private const string AppName = "ChatSystem";

        private const string ProfileDirName = "Profiles";

        private const string DbFileName = "chatsystem.db";

        public void SetActiveProfile(string login) {
            if (string.IsNullOrEmpty(login)) {
                throw new ArgumentException("Unable to set app a database for new profile. Login cannot be empty",
                    nameof(login));
            }

            _activeLogin = login;
        }

        public string GetDatabasePath() {

            var profileDir = GetBuiltProfileDirPath();

            Directory.CreateDirectory(profileDir);
            return Path.Combine(profileDir, DbFileName);
        }

        private string GetBuiltProfileDirPath() {
            if (string.IsNullOrEmpty(_activeLogin)) {
                throw new InvalidOperationException("Cannot access database: No active user profile is set.");
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppName,
                ProfileDirName,
                _activeLogin
            );
        }

        public void ClearProfile() {
            _activeLogin = null;
        }
    }
}
