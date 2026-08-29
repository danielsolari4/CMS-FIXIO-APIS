using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.FrontendApi.Controllers.ExceptionController;
using Ray.Managers;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Author")]
    public class AuthorController : BaseApiController
    {
        private readonly IAuthorManager _manager;
        private readonly AppSettings _appSettings;

        public AuthorController(IAuthorManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }


                
        public async Task<IActionResult> Get(int id, bool includeNews = false, bool includeMedia = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                var author = await _manager.GetById(id, includeNews, includeMedia);

                if (author == null)
                    return NotFound();

                return Ok(CMSResponse(author));
            });

        }

                
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeNews = false, bool includeMedia = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(
                    await _manager.GetAll(pagination.Skip(), pagination.Take(), includeNews, includeMedia),
                    pagination));
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]        
        
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(SolrCore.AUTHOR,
                HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString())), _appSettings.Solr)));
        }

        public async Task<IActionResult> Post(AuthorDto authorDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                var result = await _manager.Add(authorDto);

                return Ok(CMSResponse(result));
            });
        }
    }
}