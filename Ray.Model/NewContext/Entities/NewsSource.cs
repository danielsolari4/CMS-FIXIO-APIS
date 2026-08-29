using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class NewsSource
    {
        public NewsSource()
        {
            News = new HashSet<News>();
            Nodes = new HashSet<Node>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }

        public virtual ICollection<News> News { get; set; }
        public virtual ICollection<Node> Nodes { get; set; }
    }
}
