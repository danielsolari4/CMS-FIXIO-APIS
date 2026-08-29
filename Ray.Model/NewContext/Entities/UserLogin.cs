using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class UserLogin: IdentityUserLogin<int>
    {
        public virtual User User { get; set; }
    }
}
