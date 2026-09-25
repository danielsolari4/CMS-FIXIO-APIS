using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class Language
    {
        public Language()
        {
            AssetContents = new HashSet<AssetContent>();
            NodeContents = new HashSet<NodeContent>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string CultureName { get; set; }
        public string DisplayName { get; set; }

        public virtual ICollection<AssetContent> AssetContents { get; set; }
        public virtual ICollection<NodeContent> NodeContents { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
