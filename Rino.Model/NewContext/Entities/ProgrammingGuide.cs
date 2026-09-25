using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class ProgrammingGuide
    {
        public ProgrammingGuide()
        {
            ProgramGalleries = new HashSet<ProgramGallery>();
            ProgrammingGuideMedia = new HashSet<ProgrammingGuideMedia>();
            ProgrammingGuideNodes = new HashSet<ProgrammingGuideNode>();
            ProgrammingGuideSchedules = new HashSet<ProgrammingGuideSchedule>();
            OnCreated();
        }

        partial void OnCreated();

        public int Id { get; set; }
        public string ProgramName { get; set; }
        public string Url { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string LastModificationUser { get; set; }
        public string MobileUrl { get; set; }
        public string SecondaryUrl { get; set; }
        public int Channel { get; set; }
        public bool? CacheSolr { get; set; }
        public bool Featured { get; set; }
        public int? NodeId { get; set; }

        public virtual Channel ChannelNavigation { get; set; }
        public virtual Node Node { get; set; }
        public virtual ICollection<ProgramGallery> ProgramGalleries { get; set; }
        public virtual ICollection<ProgrammingGuideMedia> ProgrammingGuideMedia { get; set; }
        public virtual ICollection<ProgrammingGuideNode> ProgrammingGuideNodes { get; set; }
        public virtual ICollection<ProgrammingGuideSchedule> ProgrammingGuideSchedules { get; set; }
    }
}
