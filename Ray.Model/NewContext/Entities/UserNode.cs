using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class UserNode
    {
        public int UserId { get; set; }
        public int NodeId { get; set; }

        public virtual Node Node { get; set; }
        public virtual User User { get; set; }
    }
}
