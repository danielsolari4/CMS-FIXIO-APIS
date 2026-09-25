using Rino.Model.NewContext.Entities;
using System.Linq;

namespace Rino.Dtos.Mapping
{
    public static class RoleMap
    {
        public static RoleDto Map (this Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Actions = role.RoleActions.Select(s => s.Action?.Map()).ToList()
            };
        }

        public static Role Map (this RoleDto role)
        {
            return new Role
            {
                Id = role.Id,
                Name = role.Name
               // Actions = role.Actions.Select(x => x.Map()).ToList()
            };
        }
    }

    public static class ActionMap
    {
        public static ActionDto Map (this Action action)
        {
            return new ActionDto
            {
                Id = action.Id,
                ActionName = action.ActionName,
                ControllerName = action.ControllerName,
                Code = action.Code,
                Name = action.Name,
                GroupActionId = action.GroupActionId,
            };
        }

        public static Action Map (this ActionDto action)
        {
            return new Action()
            {
                Id = action.Id,
                ActionName = action.ActionName,
                ControllerName = action.ControllerName,
                Code = action.Code,
                Name = action.Name
                
            };
        }
    }
}
