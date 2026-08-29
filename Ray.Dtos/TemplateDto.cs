using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class TemplateDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "TemplateId")]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Class { get; set; }

        public TemplateDto()
        {
        }
    }
}
