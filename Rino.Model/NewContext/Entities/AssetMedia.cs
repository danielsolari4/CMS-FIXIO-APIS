using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class AssetMedia
    {
        public int AssetId { get; set; }
        public int MediaId { get; set; }
        public bool Featured { get; set; }

        public virtual Asset Asset { get; set; }
        public virtual Media Media { get; set; }
    }
}
