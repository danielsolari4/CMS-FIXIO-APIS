using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class GalleryDto : BaseDto
    {
        [StringLength(100, ErrorMessage = "Name max length is 100")]
        public virtual string Name { get; set; }

        public bool IsEnabled { get; set; }

        public List<MediaDto> Media { get; set; }

        public List<ComponentInstanceDto> ComponentInstances { get; set; }
        public int Order { get; set; }

        public GalleryDto()
        {
            Media = new List<MediaDto>();
            ComponentInstances = new List<ComponentInstanceDto>();
        }
    }

    public class CreateGalleryDtoBindingModel : GalleryDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name max length is 100")]
        public override string Name { get; set; }
    }

    public class UpdateGalleryDtoBindingModel : CreateGalleryDtoBindingModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }

    public class DeleteGalleryDtoBindingModel : GalleryDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
