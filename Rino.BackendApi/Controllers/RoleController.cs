using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.BackendApi.Attributes;
using Rino.Dtos;
using Rino.Managers;
using Rino.Repositories;
using Rino.Utils.Exception;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Role")]
    public class RoleController : BaseApiController
    {
        private readonly IRoleManager _manager;
        private readonly IRoleRepository _repository;


        public RoleController(IRoleManager manager, IRoleRepository repository)
        {
            _manager = manager;
            _repository = repository;
        }

        [HttpGet, Route("GetAll")]

        public async Task<IActionResult> GetAll()
        {
            return await TryJsonResultAsync(async () =>
            {
                var listRoles = await _manager.GetAll();
                var result = listRoles.OrderBy(x => x.Id);
                    
                return Ok(CMSResponse(result));
            });
        }

        [HttpGet]
        [Route("GetAllGroups")]
        public async Task<IActionResult> GetAllGroups()
        {
           
            return await TryJsonResultAsync(async () =>
            {                   
                return Ok(CMSResponse(_manager.GetGroups()));
            });
        }


        [HttpGet]
        [Route("GetRoleById")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var result = await _manager.GetById(id);
                return Ok(CMSResponse(result));
            });
        }

        [HttpPost, Route("Post")]
        public async Task<IActionResult> Post(RoleDto role)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (_repository.Get(x => x.Name.ToLower() == role.Name.ToLower()).Result.Any())
                    throw new AlreadyExistsException("role"); 

                var result = await _manager.Add(role);

                return Ok(CMSResponse(result));
            });
        }

        [HttpPut]
        public async Task<IActionResult> Put(RoleDto role)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!_repository.Get(x => x.Id == role.Id).Result.Any())
                    throw new ItemNotFoundException("role");

                await _manager.Update(role);

                return Ok();
            });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(RoleDto role)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!_repository.Get(x => x.Id == role.Id).Result.Any())
                    throw new ItemNotFoundException("role");

                await _manager.Delete(role);

                return Ok();
            });
        }
    }
}