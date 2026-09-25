using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class ProgrammingGuideNode
    {
        public int ProgrammingGuideId { get; set; }
        public int NodeId { get; set; }

        public virtual Node Node { get; set; }
        public virtual ProgrammingGuide ProgrammingGuide { get; set; }
    }
}
