
using System.Collections.Generic;

namespace Rino.Dtos
{
    public class ActionDto
    {
        public int Id { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int GroupActionId { get; set; }
    }


    public class GroupActionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public GroupDto Group { get; set; }
    }

    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GroupActionDto> GroupAction { get; set; }
    }
}
