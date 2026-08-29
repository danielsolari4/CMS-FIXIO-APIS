using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class AssetContent
    {

        public int Id { get; set; }
        public int AssetId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Discriminator { get; set; }
        public int LanguageId { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string Introduction { get; set; }
        public string Volanta { get; set; }
        public string MobileTitle { get; set; }
        public int? TemplateId { get; set; }
        public string SocialNetworkTitle { get; set; }

        public virtual Asset Asset { get; set; }
        public virtual Language Language { get; set; }
        public virtual Template Template { get; set; }
        //public virtual PageContent PageContent { get; set; }
    }
}
