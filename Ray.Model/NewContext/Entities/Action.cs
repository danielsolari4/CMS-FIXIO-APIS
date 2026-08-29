using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Action
    {
        public Action()
        {
            RoleActions = new HashSet<RoleAction>();
            WorkflowStepActions = new HashSet<WorkflowStepAction>();
        }

        public int Id { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int GroupActionId { get; set; }

        public virtual GroupAction GroupAction { get; set; }
        public virtual ICollection<RoleAction> RoleActions { get; set; }
        public virtual ICollection<WorkflowStepAction> WorkflowStepActions { get; set; }
    }
}
