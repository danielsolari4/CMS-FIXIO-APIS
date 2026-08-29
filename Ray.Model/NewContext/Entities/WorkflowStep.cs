using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class WorkflowStep
    {
        public WorkflowStep()
        {
            WorkflowStepActions = new HashSet<WorkflowStepAction>();
            WorkflowStepSettings = new HashSet<WorkflowStepSetting>();
        }

        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }

        public virtual Workflow Workflow { get; set; }
        public virtual ICollection<WorkflowStepAction> WorkflowStepActions { get; set; }
        public virtual ICollection<WorkflowStepSetting> WorkflowStepSettings { get; set; }
    }
}
