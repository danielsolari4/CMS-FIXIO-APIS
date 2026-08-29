using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class PrintEditionNew
    {
        public int Id { get; set; }
        public int PrintEditionId { get; set; }
        public int NewId { get; set; }
        public string Page { get; set; }

        public virtual News New { get; set; }
        public virtual PrintEdition PrintEdition { get; set; }
    }
}
