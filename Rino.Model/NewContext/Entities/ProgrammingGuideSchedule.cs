using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class ProgrammingGuideSchedule
    {
        public int Id { get; set; }
        public int ProgrammingGuideId { get; set; }
        public int InitHour { get; set; }
        public int InitMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public int Day { get; set; }

        public virtual ProgrammingGuide ProgrammingGuide { get; set; }
    }
}
