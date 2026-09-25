using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class NodeKeyword
    {
        public int NodeId { get; set; }
        public int KeywordId { get; set; }

        public virtual Keyword Keyword { get; set; }
        public virtual Node Node { get; set; }
    }
}
