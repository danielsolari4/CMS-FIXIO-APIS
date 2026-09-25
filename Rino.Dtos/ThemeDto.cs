using System.ComponentModel.DataAnnotations;

namespace Rino.Dtos
{
    public class ThemeDto : BaseDto
    {
        public string Structure { get; set; }
    }

    public class UpdateThemeDtoBindingModel : ThemeDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }        
        public string Structure { get; set; }
    }
}
