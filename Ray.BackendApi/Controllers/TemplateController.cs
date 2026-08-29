using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.BackendApi.Attributes;
using Ray.Managers;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Template")]
    public class TemplateController : BaseApiController
    {
        private readonly ITemplateManager _manager;

        public TemplateController(ITemplateManager manager)
        {
            _manager = manager;
        }
         [HttpGet]
         [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var layout = await _manager.GetById(id);

                    if (layout == null)
                        return NotFound();

                    return Ok(CMSResponse(layout));
                });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(CMSResponse(await _manager.GetAll()));
                });
        }

    }
}
