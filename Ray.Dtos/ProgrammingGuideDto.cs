using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class ProgrammingGuideDto : BaseDto
    {
        public virtual string ProgramName { get; set; }

        public virtual List<ProgrammingGuideScheduleDto> Schedule { get; set; }

        public virtual string Url { get; set; }

        public string MobileUrl { get; set; }

        public string SecondaryUrl { get; set; }

        public virtual int ChannelId { get; set; }

        public virtual string Channel { get; set; }

        public bool IsEnabled { get; set; }
        public bool Featured { get; set; }

        public List<MediaDto> Media { get; set; }

        public List<NodeDto> Nodes { get; set; }
        public NodeDto Node { get; set; }

        public ProgrammingGuideDto()
        {
            Media = new List<MediaDto>();
            Nodes = new List<NodeDto>();
            Schedule = new List<ProgrammingGuideScheduleDto>();
        }

        public List<GalleryDto> Galleries { get; set; }
    }

    public class CreateProgrammingGuideDtoBindingModel : ProgrammingGuideDto
    {
        [Required]
        public override List<ProgrammingGuideScheduleDto> Schedule { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name max length is 100")]
        public override string ProgramName { get; set; }

        [StringLength(256, ErrorMessage = "Url max length is 256")]
        public override string Url { get; set; }

        //[Required]
        //[StringLength(100, ErrorMessage = "Channel max length is 100")]
        public override int ChannelId { get; set; }

        public CreateProgrammingGuideDtoBindingModel()
        {
            Schedule = null;
        }
    }

    public class UpdateProgrammingGuideDtoBindingModel : CreateProgrammingGuideDtoBindingModel
    {
        [Required]
        public override List<ProgrammingGuideScheduleDto> Schedule { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        public UpdateProgrammingGuideDtoBindingModel()
        {
            Schedule = null;
        }
    }

    public class DeleteProgrammingGuideDtoBindingModel : ProgrammingGuideDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
