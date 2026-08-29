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
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Layout")]
    public class LayoutController : BaseApiController
    {
        private readonly ILayoutManager _manager;
        private readonly AppSettings _appSettings;

        public LayoutController(ILayoutManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        [HttpPost]
        public async Task<IActionResult> Post(LayoutDto layoutDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());
                if(layoutDto.Structure == null)
                    throw new ModelException("Empty Structure");

                return Ok(CMSResponse(await _manager.Add(layoutDto)));
            });
        }

        [HttpPut]
        public async Task<IActionResult> Put(LayoutDto layoutDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                if (layoutDto.Structure == null)
                    throw new ModelException("Empty Structure");

                await _manager.Update(layoutDto);

                return Ok();
            });
        }
        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(LayoutDto templates)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Delete(templates);

                return Ok();
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.LAYOUT,
                    HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr));
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var layout = await _manager.GetById(id);

                    if (layout == null)
                        return NotFound();

                    return Ok(CMSResponse(layout));
                });
        }


        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take()), pagination));
                });
        }

    }
}

