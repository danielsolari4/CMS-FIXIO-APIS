using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Menu
    {
        public int Id { get; set; }
        public int MenuType { get; set; }
        public string Structure { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string LastModificationUser { get; set; }
    }
}
