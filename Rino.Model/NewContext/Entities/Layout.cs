using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class Layout
    {
        public Layout()
        {
            LayoutInstances = new HashSet<LayoutInstance>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Structure { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool CacheSolr { get; set; }
        public int Type { get; set; }

        public virtual ICollection<LayoutInstance> LayoutInstances { get; set; }
    }
}
