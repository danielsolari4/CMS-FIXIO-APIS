using System.ComponentModel.DataAnnotations;

namespace Rino.Dtos
{
    public class KeywordDto
    {
        public virtual int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class DeleteKeywordDtoBindingModel : KeywordDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }

    public class SetFeaturedKeywordsDtoBindingModel
    {
        [Required]
        public string KeywordIds { get; set; }
    }
}
