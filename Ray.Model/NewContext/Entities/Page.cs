using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Page : Asset
    {
        public bool WasPublished { get; set; }
        public string PublicationUser { get; set; }
        public DateTime? PublicationDate { get; set; }
        public int ViewsCount { get; set; }
        public int? VotesCount { get; set; }
        public int? CommentsCount { get; set; }
        public string Url { get; set; }
        public int ShareCount { get; set; }
        public bool IsPrivate { get; set; }
        public string ShortUrl { get; set; }
        public bool IsAlert { get; set; }

        //public virtual Asset IdNavigation { get; set; }
        //public virtual News News { get; set; }
    }
}
