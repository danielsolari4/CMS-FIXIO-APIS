using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.Dtos.Configuration;
using Rino.Managers;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Node")]
    public class NodeController : BaseApiController
    {
        private readonly INodeManager _manager;
        private readonly AppSettings _appSettings;

        public NodeController(INodeManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }


        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, bool includeContent = false, bool includeAssets = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                var node = await _manager.GetById(id, includeContent, includeAssets, languageId);

                if (node == null)
                    return NotFound();

                return Ok(CMSResponse(node));
            });
        }

        [HttpGet, Route("GetAllSolr")]
        
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.NODE, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr));
            });
        }

        [HttpGet, Route("GetTree")]
        

        public async Task<IActionResult> GetTree(int nodeId = 0, bool includeContent = true, bool onlyActive=false,int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(CMSResponse(_manager.GetTree(nodeId, includeContent, onlyActive, languageId)));
            });
        }


        [HttpGet, Route("GetTreeMicrosite")]
        

        public async Task<IActionResult> GetTreeMicrosite(int nodeId = 0, bool includeContent = true, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(CMSResponse(_manager.GetTreeMicrosite(nodeId, includeContent, languageId)));
            });
        }
    }

}

