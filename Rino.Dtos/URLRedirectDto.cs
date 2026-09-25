using System.ComponentModel.DataAnnotations;

namespace Rino.Dtos
{
    public class URLRedirectDto : BaseDto
    {
        public string From { get; set; }
        public string To { get; set; }

        public URLRedirectDto() { }
    }

    public class UpdateUrlRedirectDtoBindingModel : URLRedirectDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
