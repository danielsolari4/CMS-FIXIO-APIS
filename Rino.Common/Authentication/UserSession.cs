using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Common.Authentication
{
    public interface ICurrentUserService
    {
        IUserSession GetCurrentUser();
    }

    public class UserSession : IUserSession
    {
        public string UserName { get; set; }

        public bool IsAuthenticated { get; set; }
    }

    public interface IUserSession
    {
        string UserName { get; set; }

        bool IsAuthenticated { get; set; }
    }
}
