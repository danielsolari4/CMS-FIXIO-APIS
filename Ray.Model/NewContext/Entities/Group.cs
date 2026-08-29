using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Group
    {
        public Group()
        {
            GroupActions = new HashSet<GroupAction>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<GroupAction> GroupActions { get; set; }
    }
}
