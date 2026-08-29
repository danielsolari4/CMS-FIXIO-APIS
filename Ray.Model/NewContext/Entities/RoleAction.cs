using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class RoleAction
    {
        public int RoleId { get; set; }
        public int ActionId { get; set; }

        public virtual Action Action { get; set; }
        public virtual Role Role { get; set; }
    }
}
