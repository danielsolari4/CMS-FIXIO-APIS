using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ray.BackendApi.Attributes;
using Ray.Dtos;
using Ray.Dtos.JsonEntities;
using Ray.Managers;
using Ray.Repositories;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Theme")]
    public class ThemeController : BaseApiController
    {
        private readonly IThemeManager _manager;
        private readonly INodeManager _nodeManager;
        private readonly IThemeRepository _themeRepository;

        public ThemeController(IThemeManager manager,
                                     INodeManager nodeManager,
                                     IThemeRepository themeRepository)
        {
            _manager = manager;
            _nodeManager = nodeManager;
            _themeRepository = themeRepository;
        }

        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var theme = await _manager.Get();

                if (theme == null)
                    return NotFound();

                return Ok(CMSResponse(theme));
            });

        }

        public async Task<IActionResult> GetAll(PaginationDto pagination)
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(CMSResponse(
                    await _manager.GetAll()));
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                var theme = await _manager.Get();

                if (theme == null)
                    return NotFound();

                return Ok(CMSResponse(theme));
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ThemeJson data)
        {
            if (data == null)
            {
                return LegacyBadRequest("Model Required");
            }
            else
            {
                await _manager.Add(new ThemeDto
                {
                    Structure = JsonConvert.SerializeObject(data)
                });

                return Ok(CMSResponse(null));
            }
        }
    }
}
