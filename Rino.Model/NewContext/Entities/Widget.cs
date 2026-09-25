using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Html { get; set; }
        public int WidgetTypeId { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool? CacheSolr { get; set; }
        public string Icon { get; set; }

        public virtual WidgetType WidgetType { get; set; }
    }
}
