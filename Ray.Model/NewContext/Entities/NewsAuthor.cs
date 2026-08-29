using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class NewsAuthor
    {
        public int NewsId { get; set; }
        public int AuthorId { get; set; }

        public virtual Author Author { get; set; }
        public virtual News News { get; set; }
    }
}
