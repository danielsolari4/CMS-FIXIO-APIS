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
    [Route("api/Page")]
    public class PageController : BaseApiController
    {
        private readonly IPageManager _manager;
        private readonly AppSettings _appSettings;

        public PageController(IPageManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        public async Task<IActionResult> Get(int id, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var page = await _manager.GetById(id, includeContent, includeNodes, includeMedia, includeGalleries, languageId);

                    if (page == null)
                        return NotFound();

                    return Ok(CMSResponse(page));
                });
        }

        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll(includeContent, includeNodes, includeMedia, includeGalleries, languageId, pagination.Skip(), pagination.Take()), pagination));
                });
        }

        [HttpGet, Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.PAGE, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr));
                });
        }

        [HttpGet, Route("GetByNodeId")]
        public async Task<IActionResult> GetByNodeId(int id, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                var page = await _manager.GetByNodeId(id, includeContent, includeNodes, includeMedia, includeGalleries, languageId);

                //if (page == null)
                //    return NotFound();

                return Ok(CMSResponse(page));
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreatePageDtoBindingModel pageDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var result = await _manager.Add(pageDto);

                    return Ok(CMSResponse(result));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UpdatePageDtoBindingModel pageDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(pageDto);

                    return Ok();
                });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(DeletePageDtoBindingModel pageDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Delete(pageDto);

                    return Ok();
                });
        }

        [HttpPut]
        [Route("ShareCount")]
        public async Task<IActionResult> ShareCount(CountDto dto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    dto.Discriminator = CountDiscriminator.Share;
                    await _manager.UpdateCounts(dto);

                    return Ok();
                });
        }

        [HttpPut]
        [Route("ViewsCount")]
        public async Task<IActionResult> ViewsCount(CountDto dto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    dto.Discriminator = CountDiscriminator.Views;
                    await _manager.UpdateCounts(dto);

                    return Ok();
                });
        }
    }
}
