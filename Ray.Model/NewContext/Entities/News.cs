using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class News : Page
    {
        public News()
        {
            NewsAuthors = new HashSet<NewsAuthor>();
            PrintEditionNews = new HashSet<PrintEditionNew>();
            Discriminator = "News";
        }

        public DateTime? ExpirationDate { get; set; }
        public string EditorComments { get; set; }
        public int? NewsSourceId { get; set; }
        public string NewsSourceNewsId { get; set; }

        //public virtual Page IdNavigation { get; set; }
        public virtual NewsSource NewsSource { get; set; }
        public virtual ICollection<NewsAuthor> NewsAuthors { get; set; }
        public virtual ICollection<PrintEditionNew> PrintEditionNews { get; set; }
    }
}
