using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class NodeContent
    {
        public int Id { get; set; }
        public int NodeId { get; set; }
        public string Title { get; set; }
        public int LanguageId { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }

        public virtual Language Language { get; set; }
        public virtual Node Node { get; set; }
    }
}
