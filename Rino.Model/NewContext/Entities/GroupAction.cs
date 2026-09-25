using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class GroupAction
    {
        public GroupAction()
        {
            Actions = new HashSet<Action>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? GroupId { get; set; }

        public virtual Group Group { get; set; }
        public virtual ICollection<Action> Actions { get; set; }
    }
}
