using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class AssetAsset
    {
        public int AssetId { get; set; }
        public int RelatedAssetId { get; set; }

        public virtual Asset Asset { get; set; }
        public virtual Asset RelatedAsset { get; set; }
    }
}
