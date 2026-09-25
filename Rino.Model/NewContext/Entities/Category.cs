using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class Category
    {
        public Category()
        {
            CategoryNodes = new HashSet<CategoryNode>();
            Childs = new HashSet<Category>();
            MediaCategories = new HashSet<MediaCategory>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string LastModificationUser { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool? CacheSolr { get; set; }

        public virtual Category Parent { get; set; }
        public virtual ICollection<CategoryNode> CategoryNodes { get; set; }
        public virtual ICollection<Category> Childs { get; set; }
        public virtual ICollection<MediaCategory> MediaCategories { get; set; }
    }
}
