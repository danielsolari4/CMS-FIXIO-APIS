using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class LayoutInstance
    {
        public int Id { get; set; }
        public int? LayoutId { get; set; }
        public int LayoutType { get; set; }
        public int NodeId { get; set; }
        public string Description { get; set; }
        public string Structure { get; set; }
        public bool IsEnabled { get; set; }
        public bool CacheSolr { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public DateTime PublicationDate { get; set; }
        public bool IsDeleted { get; set; }
        public int? PrintEditionId { get; set; }

        public virtual Layout Layout { get; set; }
        public virtual Node Node { get; set; }
        public virtual PrintEdition PrintEdition { get; set; }
    }
}
