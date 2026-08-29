using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class ProgrammingGuideMedia
    {
        public int ProgrammingGuideId { get; set; }
        public int MediaId { get; set; }

        public virtual Media Media { get; set; }
        public virtual ProgrammingGuide ProgrammingGuide { get; set; }
    }
}
