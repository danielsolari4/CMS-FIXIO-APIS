using System.Collections;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Dtos.JsonEntities;
using Ray.Managers;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Menu")]
    public class MenuController : BaseApiController
    {
        private readonly IMenuManager _manager;
        private readonly AppSettings _appSettings;

        public MenuController(IMenuManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        
       
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var node = await _manager.GetById(id);

                    if (node == null)
                        return NotFound();

                    return Ok(CMSResponse(node));
                });
        }

        [HttpGet]
        [Route("GetByTypeId2")]
        public async Task<IActionResult> GetByTypeId2(int typeId)
        {
            return await TryJsonResultAsync(async () =>
            {
                var node = await _manager.GetByTypeId(typeId);

                if (node == null)
                    return NotFound();

                return Ok(node);
            });
        }

        [HttpGet, Route("GetByTypeId")]
        public async Task<IActionResult> GetByTypeId(int typeId)
        {
            return await TryJsonResultAsync(async () =>
            {
                var node = await _manager.GetByTypeId(typeId);

                if (node == null)
                    return NotFound();

                return Ok(node);
            });
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll()));
                });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.NODE, HttpUtility.UrlDecode(Request.QueryString.ToString()), _appSettings.Solr));
                });
        }


     
    }
}
