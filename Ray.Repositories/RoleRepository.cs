using System.Collections.Generic;
using System.Linq;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IRoleRepository : IAsyncRepository<Role>
    {
        ICollection<GroupDto> GetGroups();
    }

    public class RoleRepository : BaseAsyncRepository<Role>, IRoleRepository
    {
        public RoleRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Roles;
        }

        public ICollection<GroupDto> GetGroups()
        {
            var gp = UnitOfWork.Context.Groups;
            return gp.Select(x => new GroupDto
            {
                Id = x.Id,
                Name = x.Name,
                GroupAction = x.GroupActions.Select(s => new GroupActionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    GroupId = s.GroupId ?? 0
                }).ToList()
            }).ToList();

        }
    }
}
