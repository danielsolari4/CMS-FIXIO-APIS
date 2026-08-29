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
    [Route("api/ProgrammingGuide")]
    public class ProgrammingGuideController : BaseApiController
    {
        private readonly IProgrammingGuideManager _manager;
        private readonly AppSettings _appSettings;

        public ProgrammingGuideController(IProgrammingGuideManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id, bool includeMedia = false, bool includeNodes = false, bool includeChannels = false)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var programmingGuide = await _manager.GetById(id, includeMedia, includeNodes, includeChannels);

                    if (programmingGuide == null)
                        return NotFound();

                    return Ok(CMSResponse(programmingGuide));
                });
        }

        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeMedia = false, bool includeNodes = false, bool includeChannels = false)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take(), includeMedia, includeNodes, includeChannels), pagination));
                });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.PROGRAMMINGGUIDE, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr));
                });
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateProgrammingGuideDtoBindingModel programmingGuideDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var result = await _manager.Add(programmingGuideDto);

                    return Ok(CMSResponse(result));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put(UpdateProgrammingGuideDtoBindingModel programmingGuideDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(programmingGuideDto);

                    return Ok();
                });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(DeleteProgrammingGuideDtoBindingModel programmingGuideDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Delete(programmingGuideDto);

                    return Ok();
                });
        }
    }
}
