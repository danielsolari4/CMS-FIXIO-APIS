using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Service
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int TimeInMinutes { get; set; }
        public int Type { get; set; }
        public bool Enable { get; set; }
    }
}
