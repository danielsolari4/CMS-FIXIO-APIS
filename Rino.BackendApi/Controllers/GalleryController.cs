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
    [Route("api/Gallery")]
    public class GalleryController : BaseApiController
    {
        private readonly IGalleryManager _manager;
        private readonly AppSettings _appSettings;

        public GalleryController(IGalleryManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id, bool includeMedia = false)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var gallery = await _manager.GetById(id, includeMedia);

                    if (gallery == null)
                        return NotFound();

                    return Ok(CMSResponse(gallery));
                });
        }

        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeMedia = false)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take(), includeMedia), pagination));
                });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(SolrCore.GALLERY, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr)));
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateGalleryDtoBindingModel galleryDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var result = await _manager.Add(galleryDto);

                    return Ok(CMSResponse(result));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put(UpdateGalleryDtoBindingModel galleryDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(galleryDto);

                    return Ok();
                });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(DeleteGalleryDtoBindingModel galleryDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage()); 

                    await _manager.Delete(galleryDto);

                    return Ok();
                });
        }
    }
}
