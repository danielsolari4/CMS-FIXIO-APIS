using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class SocialNetwork
    {
        public SocialNetwork()
        {
            PageContentSocialNetworks = new HashSet<PageContentSocialNetwork>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }

        public virtual ICollection<PageContentSocialNetwork> PageContentSocialNetworks { get; set; }
    }
}
