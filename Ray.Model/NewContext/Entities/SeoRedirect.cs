using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class SeoRedirect
    {
        public int Id { get; set; }
        public string SeoUrl { get; set; }
        public string NewUrl { get; set; }
    }
}
