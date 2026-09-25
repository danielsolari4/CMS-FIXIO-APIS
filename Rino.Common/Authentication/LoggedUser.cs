using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Common.Authentication
{
    public class LoggedUser
    {
        public string Email { get; set; }
        public int Id { get; set; }
        public List<string> Roles { get; set; }
        public List<int> RolesInt { get; set; }
    }
}
