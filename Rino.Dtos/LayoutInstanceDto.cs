using System;
using System.ComponentModel.DataAnnotations;

namespace Rino.Dtos
{
    public class LayoutInstanceDto : BaseDto
    {
        public override int Id { get; set; }
        public int? LayoutId { get; set; }
        public int LayoutType { get; set; }
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public string Description { get; set; }

        public DateTime PublicationDate { get; set; }

        public int ParentLayoutInstanceId { get; set; }

        public LayoutStructureDto StructureJson { get; set; }
        public bool CurrentPublication { get; set; }

        public int? PrintEditionId { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UpdateLayoutInstanceDtoBindingModel : LayoutInstanceDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "LayoutInstanceId")]
        public override int Id { get; set; }
    }

    public class SyncLayoutInstanceDtoBindingModel
    {
        public int? NodeId { get; set; }
        public bool ReducedData { get; set; }
    }


    public class LayoutInstanceMicrositeDto
    {
        public int Id { get; set; }
        public int NodeId { get; set; }
        public string NodeDescription { get; set; }
        public string CreationUser { get; set; }
        public string CreationDate { get; set; }
        public string LastModificationDate { get; set; }
        public string PublicationDate { get; set; }
        public string Node_en { get; set; }
        public string Node_es { get; set; }
        public bool IsDeleted { get; set; }
        public int NroRow { get; set; }
        public bool IsMicrosite { get; set; }
        public int ParentNodeId { get; set; }
        public bool HasParentMicrosite { get; set; }
        public int LevelNode { get; set; }
    }
}
