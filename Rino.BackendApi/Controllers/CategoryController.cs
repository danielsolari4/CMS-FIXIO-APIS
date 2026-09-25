using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.BackendApi.Attributes;
using Rino.BackendApi.Controllers.ExceptionController;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Category")]
    public class CategoryController : BaseApiController
    {
        private readonly ICategoryManager _manager;
        private readonly AppSettings _appSettings;

        public CategoryController(ICategoryManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id, bool includeMedia = false, bool includeNodes = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                var category = await _manager.GetById(id, includeMedia, includeNodes);

                if (category == null)
                    return NotFound();

                return Ok(CMSResponse(category));
            });
        }
        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeMedia = false, bool includeNodes = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(
                    await _manager.GetAll(pagination.Skip(), pagination.Take(), includeMedia, includeNodes),
                    pagination));
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(SolrCore.CATEGORY,
                HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString())), _appSettings.Solr)));
        }

        [HttpGet, Route("GetTree")]
        public async Task<IActionResult> GetTree(bool includeNodes = true)
        {
            return await TryJsonResultAsync(async () => Ok(CMSResponse(await _manager.GetTree(includeNodes))));
        }
        [HttpPost]
        public async Task<IActionResult> Post(CreateCategoryDtoBindingModel categoryDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                var result = await _manager.Add(categoryDto);

                return Ok(CMSResponse(result));
            });
        }
        [HttpPut]
        public async Task<IActionResult> Put(UpdateCategoryDtoBindingModel categoryDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Update(categoryDto);

                return Ok();
            });
        }
        [HttpDelete]
        [HttpPost, Route("Delete")]
        public async Task<IActionResult> Delete(DeleteCategoryDtoBindingModel categoryDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Delete(categoryDto);

                return Ok();
            });
        }
    }
}
