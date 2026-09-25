using System.Collections.Generic;

namespace Rino.Dtos
{
    public class WidgetDto : BaseDto
    {

        public string Html { get; set; }
        public int WidgetTypeId { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public string Icon { get; set; }
        public WidgetTypeDto WidgetType { get; set; }
        public bool? CacheSolr { get; set; }
        public ICollection<WidgetTypeDto> WidgetTypes { get; set; }
    }
}