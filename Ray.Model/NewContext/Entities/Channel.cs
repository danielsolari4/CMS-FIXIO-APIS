using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Channel
    {
        public Channel()
        {
            ProgrammingGuides = new HashSet<ProgrammingGuide>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsDeleted { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string LastModificationUser { get; set; }
        public bool? CacheSolr { get; set; }
        public int? MediaId { get; set; }

        public virtual Media Media { get; set; }
        public virtual ICollection<ProgrammingGuide> ProgrammingGuides { get; set; }
    }
}
