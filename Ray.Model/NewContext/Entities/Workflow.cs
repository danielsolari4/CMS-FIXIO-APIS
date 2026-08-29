using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Workflow
    {
        public Workflow()
        {
            WorkflowSettings = new HashSet<WorkflowSetting>();
            WorkflowSteps = new HashSet<WorkflowStep>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }

        public virtual ICollection<WorkflowSetting> WorkflowSettings { get; set; }
        public virtual ICollection<WorkflowStep> WorkflowSteps { get; set; }
    }
}
