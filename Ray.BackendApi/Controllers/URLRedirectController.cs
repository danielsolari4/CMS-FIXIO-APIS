using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.BackendApi.Attributes;
using Ray.BackendApi.Controllers.ExceptionController;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Repositories;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/URLRedirect")]
    public class URLRedirectController : BaseApiController
    {
        private readonly IURLRedirectManager _manager;
        private readonly INodeRepository _nodeRepository;
        private readonly AppSettings _appSettings;

        public URLRedirectController(IURLRedirectManager manager, 
                                     INodeRepository nodeRepository, AppSettings appSettings)
        {
            _manager = manager;
            _nodeRepository = nodeRepository;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var urlRedirect = await _manager.GetById(id);

                if (urlRedirect == null)
                    return NotFound();

                return Ok(CMSResponse(urlRedirect));
            });

        }

        [HttpGet, Route("GetAll")]
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
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(SolrCore.URLREDIRECT,
                HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString())),_appSettings.Solr)));
        }

        [HttpPost]
        public async Task<IActionResult> Post(URLRedirectDto urlRedirectDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                //Validate Node entity (Node)
                if (_nodeRepository.Get(x => x.IsDeleted == false && x.Description == urlRedirectDto.From).Result.Any())
                    throw new AlreadyExistsException("redirect");

                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                var result = await _manager.Add(urlRedirectDto);

                return Ok(CMSResponse(result));
            });
        }

        [HttpPut]
        public async Task<IActionResult> Put(URLRedirectDto urlRedirectDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Update(urlRedirectDto);

                return Ok();
            });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(URLRedirectDto urlRedirectDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Delete(urlRedirectDto);

                return Ok();
            });
        }
    }
}
