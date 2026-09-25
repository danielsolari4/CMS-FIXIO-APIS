using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class TimeZone
    {
        public TimeZone()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string DisplayName { get; set; }
        public double Utcoffset { get; set; }
        public string Abbr { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
