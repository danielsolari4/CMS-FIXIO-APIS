using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class PageContentSocialNetwork
    {
        public int PageContentId { get; set; }
        public int SocialNetworkId { get; set; }
        public string Url { get; set; }

        public virtual PageContent PageContent { get; set; }
        public virtual SocialNetwork SocialNetwork { get; set; }
    }
}
