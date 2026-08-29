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
    [Route("api/Keyword")]
    public class KeywordController : BaseApiController
    {
        private readonly IKeywordManager _manager;
        private readonly AppSettings _appSettings;

        public KeywordController(IKeywordManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

                
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
                await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.KEYWORD, HttpUtility.UrlDecode(Request.QueryString.ToString()), _appSettings.Solr));
            });
        }


    }
}