using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class WorkflowStepAction
    {
        public int WorkflowStepId { get; set; }
        public int ActionId { get; set; }

        public virtual Action Action { get; set; }
        public virtual WorkflowStep WorkflowStep { get; set; }
    }
}
