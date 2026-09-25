using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class PageContentKeyword
    {
        public int PageContentId { get; set; }
        public int KeywordId { get; set; }

        public virtual Keyword Keyword { get; set; }
        public virtual PageContent PageContent { get; set; }
    }
}
