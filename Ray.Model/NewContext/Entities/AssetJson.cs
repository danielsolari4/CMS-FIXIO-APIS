using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class AssetJson
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string Json { get; set; }

        public virtual Asset Asset { get; set; }
    }
}
