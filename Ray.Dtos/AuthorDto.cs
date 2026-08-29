using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class AuthorDto : BaseDto
    {
        [StringLength(256, ErrorMessage = "Email max length is 256")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "FirstName max length is 100")]
        public string FirstName { get; set; }

        [StringLength(100, ErrorMessage = "LastName max length is 100")]
        public string LastName { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsDeleted { get; set; }

        public List<NewsDto> News { get; set; }

        public MediaDto Media { get; set; }

        public AuthorDto()
        {
            News = new List<NewsDto>();
        }
    }

    public class UpdateAuthorDtoBindingModel : AuthorDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
