using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Category")]
    public class CategoryController : BaseApiController
    {
        private readonly ICategoryManager _manager;
        private readonly AppSettings _appSettings;

        public CategoryController(ICategoryManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

              
        public async Task<IActionResult> Get(int id, bool includeMedia = false, bool includeNodes = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                var category = await _manager.GetById(id, includeMedia, includeNodes);

                if (category == null)
                    return NotFound();

                return Ok(CMSResponse(category));
            });
        }

                
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeMedia = false, bool includeNodes = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(
                    await _manager.GetAll(pagination.Skip(), pagination.Take(), includeMedia, includeNodes),
                    pagination));
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(SolrCore.CATEGORY,
                HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString())), _appSettings.Solr)));
        }

        [Route("GetTree")]
        
        public async Task<IActionResult> GetTree(bool includeNodes = true)
        {
            return await TryJsonResultAsync(async () => Ok(CMSResponse(await _manager.GetTree(includeNodes))));
        }




    }
}