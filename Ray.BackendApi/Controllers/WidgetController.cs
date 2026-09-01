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
    [Route("api/Widget")]
    public class WidgetController : BaseApiController
    {
        private readonly IWidgetManager _manager;
        private readonly AppSettings _appSettings;

        public WidgetController(IWidgetManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var Widget = await _manager.GetById(id);

                if (Widget == null)
                    return NotFound();

                Widget.WidgetTypes = await _manager.GetAllTypes();

                return Ok(CMSResponse(Widget));
            });
        }

        [HttpGet]
        [Route("GetWidgetTypes")]
        public async Task<IActionResult> GetWidgetTypes()
        {
            return await TryJsonResultAsync(async () =>
            {
                var widgetTypes = await _manager.GetAllTypes();

                return Ok(CMSResponse(widgetTypes));
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

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.WIDGET, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr));
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WidgetDto widgetDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                var result = await _manager.Add(widgetDto);

                return Ok(CMSResponse(result));
            });
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] WidgetDto widgetDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Update(widgetDto);

                return Ok();
            });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(WidgetDto widgetDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Delete(widgetDto);

                return Ok();
            });
        }

        public enum WidgetType
        {
            IFrame = 1,
            Html = 2
        }
    }
}