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
    [Route("api/PrintEdition")]
    public class PrintEditionController : BaseApiController
    {
        private readonly IPrintEditionManager _manager;
        private readonly INewsManager _managerNews;
        private readonly AppSettings _appSettings;

        public PrintEditionController(IPrintEditionManager manager, INewsManager managerNews, AppSettings appSettings)
        {
            _manager = manager;
            _managerNews = managerNews;
            _appSettings = appSettings;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var PrintEdition = await _manager.GetById(id);

                if (PrintEdition == null)
                    return NotFound();



                return Ok(CMSResponse(PrintEdition));
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

        [HttpGet]
        [Route("GetAllSolr")]
        
        
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.PRINTEDITION, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr));
            });
        }
    }
}
