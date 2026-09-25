using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class Author
    {
        public Author()
        {
            NewsAuthors = new HashSet<NewsAuthor>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsEnabled { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public bool IsDeleted { get; set; }
        public int? MediaId { get; set; }
        public bool? CacheSolr { get; set; }

        public virtual Media Media { get; set; }
        public virtual ICollection<NewsAuthor> NewsAuthors { get; set; }
    }
}
