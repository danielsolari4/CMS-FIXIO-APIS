using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Managers;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Theme")]
    public class ThemeController : BaseApiController
    {
        private readonly IThemeManager _manager;

        public ThemeController(IThemeManager manager)
        {
            _manager = manager;
        }

        
        [HttpGet]
        [HttpGet("{id:int}")]
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

    
    }
}
