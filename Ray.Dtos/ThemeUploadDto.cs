using System.Collections.Generic;

namespace Ray.Dtos
{
    public class ThemeUploadDto
    {
        public string logo { get; set; }
        public string logoMobile { get; set; }
        public string backgroundColor { get; set; }
        public string mainColor { get; set; }
        public List<ThemeDto> themes { get; set; }
    }
}
