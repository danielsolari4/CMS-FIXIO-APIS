using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class UserAsset
    {
        public int UserId { get; set; }
        public int AssetId { get; set; }

        public virtual Asset Asset { get; set; }
        public virtual User User { get; set; }
    }
}
