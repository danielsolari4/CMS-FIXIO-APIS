using System.ComponentModel.DataAnnotations;

namespace Rino.Dtos
{
    public class ProgrammingGuideScheduleDto
    {
        [Range(0, 23, ErrorMessage = "InitHour")]
        public int InitHour { get; set; }

        [Range(0, 59, ErrorMessage = "InitMinutes")]
        public int InitMinute { get; set; }

        [Range(0, 23, ErrorMessage = "EndHour")]
        public int EndHour { get; set; }

        [Range(0, 59, ErrorMessage = "EndMinutes")]
        public int EndMinute { get; set; }

        [Range(0, 6, ErrorMessage = "Day")]
        public int Day { get; set; }
    }
}