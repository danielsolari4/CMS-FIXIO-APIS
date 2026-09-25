using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class CategoryNode
    {
        public int CategoryId { get; set; }
        public int NodeId { get; set; }
        public bool Featured { get; set; }

        public virtual Category Category { get; set; }
        public virtual Node Node { get; set; }
    }
}
