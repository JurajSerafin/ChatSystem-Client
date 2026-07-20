using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Client.Core.Interfaces.Database {
    internal interface IProfilePathProvider {
        void SetActiveProfile(string login);

        string GetDatabasePath();

        void ClearProfile();
    }
}
