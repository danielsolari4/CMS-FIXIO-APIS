using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class CategoryDto : BaseDto
    {
        [StringLength(100, ErrorMessage = "Name max length is 100")]
        public virtual string Name { get; set; }

        public bool IsEnabled { get; set; }

        public List<MediaDto> Media { get; set; }

        public List<CategoryNodeDto> CategoryNode { get; set; }

        public CategoryDto ParentCategory { get; set; }

        public List<CategoryDto> Childs { get; set; }

        public CategoryDto()
        {
            Media = new List<MediaDto>();
            CategoryNode = new List<CategoryNodeDto>();
            Childs = new List<CategoryDto>();
        }
    }

    public class CreateCategoryDtoBindingModel : CategoryDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name max length is 100")]
        public override string Name { get; set; }
    }

    public class UpdateCategoryDtoBindingModel : CreateCategoryDtoBindingModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }

    public class DeleteCategoryDtoBindingModel : CategoryDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
