using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class URLRedirect
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool IsDeleted { get; set; }
        public bool CacheSolr { get; set; }
    }
}
