using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.BackendApi.Attributes;
using Rino.Dtos;
using Rino.Repositories;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Action")]
    public class ActionController : BaseApiController
    {
        private readonly IActionRepository _repository;

        public ActionController(IActionRepository repository)
        {
            _repository = repository;
        }

        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return await TryJsonResultAsync(async () =>
            {
                var result = await _repository.GetAll();
                return Ok(CMSResponse(result.Select(x => new ActionDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ActionName = x.ActionName,
                    ControllerName = x.ControllerName,
                    Code = x.Code,
                    GroupActionId = x.GroupActionId
                }).ToList().OrderBy(x => x.Id)));
            });
        }
    }
}