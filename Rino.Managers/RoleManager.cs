using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rino.Dtos;
using Rino.Dtos.Mapping;
using Rino.Managers.Core;
using Rino.Model.NewContext.Entities;
using Rino.Repositories;
using Action = Rino.Model.NewContext.Entities.Action;

namespace Rino.Managers
{
    public interface IRoleManager : IManager<RoleDto>
    {
        ICollection<GroupDto> GetGroups();
    }


    public class RoleManager : BaseManager, IRoleManager
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IActionRepository _actionRepository;

        public RoleManager(IRoleRepository roleRepository, IActionRepository actionRepository)
        {
            _roleRepository = roleRepository;
            _actionRepository = actionRepository;
        }

        private async Task<Role> MapFromDto(RoleDto roleDto, Role role = null)
        {
            Role ret;

            if (role.Id != 0)
            {
                ret = role;
                ret.RoleActions.Clear();
                foreach (var act in roleDto.Actions)
                {
                    var a = await _actionRepository.GetById(act.Id);
                    ret.RoleActions.Add(new RoleAction(){ActionId = a.Id});
                }
                ret.Name = roleDto.Name;
                return ret;
            }
            else
            {
                ret = new Role { Id = role.Id, Name = role.Name, RoleActions = new List<RoleAction>() };
                if (roleDto.Actions != null)
                {
                    foreach (var act in roleDto.Actions)
                    {
                        var a = await _actionRepository.GetById(act.Id);
                        ret.RoleActions.Add(new RoleAction() { ActionId = a.Id });
                    }
                }
                return ret;
            }
        }

        public async Task<RoleDto> Add(RoleDto dto)
        {
            try
            {
                var ent = dto.Map();

                var e = await MapFromDto(dto, ent);
                await _roleRepository.Add(e);

                return e.Map();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }

        public async Task Delete(RoleDto entity)
        {
            try
            {
                var ent = await _roleRepository.GetById(entity.Id);
                await _roleRepository.Delete(ent);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<ICollection<RoleDto>> GetAll(int? skip = null, int? take = null)
        {
            try
            {
                var roleList = new List<RoleDto>();
                var all = await _roleRepository.GetAll();
                roleList = all?.ToList().Select(x => x.Map()).ToList();
                return roleList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ICollection<GroupDto> GetGroups()
        {
            try
            {
                return _roleRepository.GetGroups();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<RoleDto> GetById(int id)
        {
            try
            {
                RoleDto role = new RoleDto();
                var r = await _roleRepository.GetById(id);
                role = r?.Map();

                return role;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task Update(RoleDto dto)
        {
            try
            {
                var ent = await _roleRepository.GetById(dto.Id);
                var e = await MapFromDto(dto, ent);
                await _roleRepository.Update(e);
            }
            catch (Exception ex)
            {

            }
        }


    }
}