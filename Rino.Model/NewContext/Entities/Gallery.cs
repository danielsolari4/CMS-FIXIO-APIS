using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class Gallery
    {
        public Gallery()
        {
            AssetGalleries = new HashSet<AssetGallery>();
            MediaGalleries = new HashSet<MediaGallery>();
            ProgramGalleries = new HashSet<ProgramGallery>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public bool? CacheSolr { get; set; }

        public virtual ICollection<AssetGallery> AssetGalleries { get; set; }
        public virtual ICollection<MediaGallery> MediaGalleries { get; set; }
        public virtual ICollection<ProgramGallery> ProgramGalleries { get; set; }
    }
}
