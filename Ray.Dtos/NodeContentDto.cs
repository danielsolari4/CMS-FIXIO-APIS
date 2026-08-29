using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class NodeContentDto : BaseDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Title max length is 100")]
        public string Title { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "LanguageId")]
        public int LanguageId { get; set; }

        public NodeContentDto() { }
    }
}
