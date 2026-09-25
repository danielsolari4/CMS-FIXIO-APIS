using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class WorkflowStepSetting
    {
        public int Id { get; set; }
        public int WorkflowStepId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }

        public virtual WorkflowStep WorkflowStep { get; set; }
    }
}
