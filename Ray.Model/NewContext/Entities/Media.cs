using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Media
    {
        public Media()
        {
            AssetMedia = new HashSet<AssetMedia>();
            Authors = new HashSet<Author>();
            Channels = new HashSet<Channel>();
            MediaCategories = new HashSet<MediaCategory>();
            MediaGalleries = new HashSet<MediaGallery>();
            MediaKeywords = new HashSet<MediaKeyword>();
            ProgrammingGuideMedia = new HashSet<ProgrammingGuideMedia>();
            OnCreated();
        }

        partial void OnCreated();

        public int Id { get; set; }
        public string Title { get; set; }
        public string FileId { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string AlternativeText { get; set; }
        public string SourcePath { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public string Metadata { get; set; }
        public string Keywords { get; set; }
        public int? Version { get; set; }
        public bool IsEnabled { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string CustomData { get; set; }
        public string SizesPaths { get; set; }
        public string MediaUrl { get; set; }
        public bool Featured { get; set; }
        public int ShareCount { get; set; }
        public int ViewsCount { get; set; }
        public string Discriminator { get; set; }
        public DateTime? PublicationDate { get; set; }
        public bool? CacheSolr { get; set; }

        public virtual ICollection<AssetMedia> AssetMedia { get; set; }
        public virtual ICollection<Author> Authors { get; set; }
        public virtual ICollection<Channel> Channels { get; set; }
        public virtual ICollection<MediaCategory> MediaCategories { get; set; }
        public virtual ICollection<MediaGallery> MediaGalleries { get; set; }
        public virtual ICollection<MediaKeyword> MediaKeywords { get; set; }
        public virtual ICollection<ProgrammingGuideMedia> ProgrammingGuideMedia { get; set; }
    }
}
