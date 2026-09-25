using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.Managers;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Cache")]
    public class CacheController : BaseApiController
    {
        private readonly ICacheInvalidationManager _cacheInvalidation;

        public CacheController(ICacheInvalidationManager cacheInvalidation)
        {
            _cacheInvalidation = cacheInvalidation;
        }

        [HttpPost]
        [Route("News/{id:int}")]
        public async Task<IActionResult> News(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var result = await _cacheInvalidation.InvalidateNewsNow(id, null, $"manual news:{id}");
                return Ok(CMSResponse(result));
            });
        }

        [HttpPost]
        [Route("Layout/{nodeId:int}")]
        public async Task<IActionResult> Layout(int nodeId)
        {
            return await TryJsonResultAsync(async () =>
            {
                var result = await _cacheInvalidation.InvalidateLayoutNow(nodeId, $"manual layout:{nodeId}");
                return Ok(CMSResponse(result));
            });
        }

        [HttpPost]
        [Route("Paths")]
        public async Task<IActionResult> Paths([FromBody] List<string> paths)
        {
            return await TryJsonResultAsync(async () =>
            {
                var result = await _cacheInvalidation.InvalidatePathsNow(paths, "manual paths");
                return Ok(CMSResponse(result));
            });
        }

        [HttpPost]
        [Route("Redirects")]
        public async Task<IActionResult> Redirects()
        {
            return await TryJsonResultAsync(async () =>
            {
                var result = await _cacheInvalidation.InvalidateRedirectsNow("manual redirects");
                return Ok(CMSResponse(result));
            });
        }
    }
}
