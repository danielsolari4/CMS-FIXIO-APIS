using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Keyword
    {
        public Keyword()
        {
            MediaKeywords = new HashSet<MediaKeyword>();
            NodeKeywords = new HashSet<NodeKeyword>();
            PageContentKeywords = new HashSet<PageContentKeyword>();
            OnCreated();
        }

        partial void OnCreated();

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool Featured { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public bool? CacheSolr { get; set; }

        public virtual ICollection<MediaKeyword> MediaKeywords { get; set; }
        public virtual ICollection<NodeKeyword> NodeKeywords { get; set; }
        public virtual ICollection<PageContentKeyword> PageContentKeywords { get; set; }
    }
}
