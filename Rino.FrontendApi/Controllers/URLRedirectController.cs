using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.FrontendApi.Controllers.ExceptionController;
using Rino.Managers;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/URLRedirect")]
    public class URLRedirectController : BaseApiController
    {
        private readonly IURLRedirectManager _manager;
        private readonly AppSettings _appSettings;

        public URLRedirectController(IURLRedirectManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        
        [HttpGet("{id:int}")]
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

        
        [HttpGet]
        [HttpGet("GetAll")]
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
                HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString())), _appSettings.Solr)));
        }
    }
}
