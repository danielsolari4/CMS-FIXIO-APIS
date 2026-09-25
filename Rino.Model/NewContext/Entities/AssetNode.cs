using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class AssetNode
    {
        public int AssetId { get; set; }
        public int NodeId { get; set; }

        public virtual Asset Asset { get; set; }
        public virtual Node Node { get; set; }
    }
}
