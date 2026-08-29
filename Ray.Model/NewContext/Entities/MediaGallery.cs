using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class MediaGallery
    {
        public int MediaId { get; set; }
        public int GalleryId { get; set; }
        public int? Order { get; set; }

        public virtual Gallery Gallery { get; set; }
        public virtual Media Media { get; set; }
    }
}
