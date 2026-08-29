using System.Collections.Generic;

namespace Ray.Dtos
{
    public class AllowedControllerActionDto
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }
    public class RoleDto : BaseDto
    {
        public override int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ActionDto> Actions { get; set; }

    }
}
