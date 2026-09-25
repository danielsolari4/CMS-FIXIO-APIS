using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class PageContent : AssetContent
    {
        public PageContent()
        {
            PageContentKeywords = new HashSet<PageContentKeyword>();
            PageContentSocialNetworks = new HashSet<PageContentSocialNetwork>();
        }

        public string Keywords { get; set; }
        public string Content { get; set; }
        public string BackgroundColor { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaAuthor { get; set; }

        //public virtual AssetContent IdNavigation { get; set; }
        public virtual ICollection<PageContentKeyword> PageContentKeywords { get; set; }
        public virtual ICollection<PageContentSocialNetwork> PageContentSocialNetworks { get; set; }
    }
}
