using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class AssetGallery
    {
        public int AssetId { get; set; }
        public int GalleryId { get; set; }

        public virtual Asset Asset { get; set; }
        public virtual Gallery Gallery { get; set; }
    }
}
