using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos;
using Ray.Managers;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Layout")]
    public class LayoutController : BaseApiController
    {
        private readonly ILayoutManager _manager;

        public LayoutController(ILayoutManager manager)
        {
            _manager = manager;
        }

        [HttpGet("{id:int}")]
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
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take()), pagination));
            });
        }
    }
}
