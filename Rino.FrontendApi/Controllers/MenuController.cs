using System.Collections;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Dtos.JsonEntities;
using Rino.Managers;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.FrontendApi.Controllers
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

        
       
        [HttpGet("{id:int}")]
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
            var result = await SolrHelper.ExecuteQuery(SolrCore.MENU, HttpUtility.UrlDecode("q=MenuType:" + typeId), _appSettings.Solr);
            var stringJson = JsonConvert.SerializeObject(result);

            SolrResponse settings = JsonConvert.DeserializeObject<SolrResponse>(stringJson);

            if (settings == null)
                return NotFound();

            var items = JsonConvert.DeserializeObject<System.Collections.Generic.List<ItemMenu>>(settings?.response?.docs[0].Structure.ToString());
            var val = settings?.response?.docs?.Count > 0 ? new MenuJson
            {
                Id = settings?.response?.docs[0].Id,
                Type = settings?.response?.docs[0].MenuType,
                Items = items
            } : null;

            return Ok(val);
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
        [HttpGet("GetAll")]
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
