using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class WorkflowSetting
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }

        public virtual Workflow Workflow { get; set; }
    }
}
