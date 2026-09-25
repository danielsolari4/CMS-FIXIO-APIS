using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class PrintEdition
    {
        public PrintEdition()
        {
            LayoutInstances = new HashSet<LayoutInstance>();
            PrintEditionNews = new HashSet<PrintEditionNew>();
        }

        public int Id { get; set; }
        public string Edition { get; set; }
        public int NodeId { get; set; }
        public string Structure { get; set; }
        public string Identifier { get; set; }
        public DateTime MigrationDate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public bool CacheSolr { get; set; }
        public bool IsPublished { get; set; }

        public virtual Node Node { get; set; }
        public virtual ICollection<LayoutInstance> LayoutInstances { get; set; }
        public virtual ICollection<PrintEditionNew> PrintEditionNews { get; set; }
    }
}
