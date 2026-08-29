using System.Collections.Generic;
using System.Linq;
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
    [Route("api/News")]
    public class NewsController : BaseApiController
    {
        private readonly INewsManager _manager;
        private readonly IUserManager _userManager;
        private readonly AppSettings _appSettings;
        private readonly IBackloadManager _backloadManager;

        public NewsController(INewsManager manager, IUserManager userManager, AppSettings appSettings, IBackloadManager backloadManager)
        {
            _manager = manager;
            _userManager = userManager;
            _appSettings = appSettings;
            _backloadManager = backloadManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id, bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var news = await _manager.GetById(id, includeContent, includeAuthors, includeNodes, includeMedia, includeGalleries, true, languageId);

                    if (news == null)
                        return NotFound();

                    #region Change Create and Modification User Info

                    UserDto userCreationInfo = null;
                    if (!string.IsNullOrEmpty(news.CreationUser))
                        userCreationInfo = _userManager.GetAll().Result.FirstOrDefault(x => x.UserName == news.CreationUser);

                    if (userCreationInfo != null)
                        news.CreationUser = userCreationInfo.FirstName + " " + userCreationInfo.LastName;
                    
                    UserDto userModificationInfo = null;
                    if (!string.IsNullOrEmpty(news.LastModificationUser))
                        userModificationInfo = _userManager.GetAll().Result.FirstOrDefault(x => x.UserName == news.LastModificationUser);

                    if (userModificationInfo != null)
                        news.LastModificationUser = userModificationInfo.FirstName + " " + userModificationInfo.LastName;

                    #endregion

                    return Ok(CMSResponse(news));
                });
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll(includeContent, includeAuthors, includeNodes, includeMedia, includeGalleries, includeRelatedAssets, languageId, pagination.Skip(), pagination.Take()), pagination));
                });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.NEWS, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr));
                });
        }

        [HttpPost]
        public async Task<IActionResult> Post(ImportNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    //TODO: ver código comentado
                    if (!string.IsNullOrEmpty(newsDto.ImportImageUrl))
                    {
                        var media = _backloadManager.UploadImageFromUrl(newsDto.ImportImageUrl, newsDto.PhotoName ?? "Importada Feed", HttpContext);
                        if (media != null && media.Id > 0)
                        {
                            newsDto.AssetMedia.Add(new AssetMediaDto { Media = media });
                            newsDto.Galleries = new List<DeleteGalleryDtoBindingModel>() {
                                   new DeleteGalleryDtoBindingModel{
                                       Name = "Galería Importada",
                                       Media = new List<MediaDto> { media }}
                                    };
                        }
                    }

                    if (newsDto.ImportImages != null && newsDto.ImportImages.Any())
                    {
                        foreach (var image in newsDto.ImportImages)
                        {
                            var media = _backloadManager.UploadImageFromUrl(image.ImportImageUrl, image.Description, HttpContext);
                            if (media != null && media.Id > 0)
                            {
                                newsDto.AssetMedia.Add(new AssetMediaDto { Media = media });
                                newsDto.Galleries = new List<DeleteGalleryDtoBindingModel>() {
                                    new DeleteGalleryDtoBindingModel{
                                        Name = "Galería Importada",
                                        Media = new List<MediaDto> { media }}
                                };
                            }
                        }

                    }

                    var result = await _manager.Add(newsDto);

                    return Ok(CMSResponse(result));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put(UpdateNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(newsDto);

                    return Ok();
                });
        }

        [Route("SendApproval")]
        public async Task<IActionResult> SendApproval(DeleteNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.SendApproval(newsDto);

                    return Ok();
                });
        }

        [Route("Approve")]
        public async Task<IActionResult> Approve(DeleteNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Approve(newsDto);

                    return Ok();
                });
        }

        [Route("Disapprove")]
        public async Task<IActionResult> Disapprove(DeleteNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Disapprove(newsDto);

                    return Ok();
                });
        }

        [Route("Publish")]
        public async Task<IActionResult> Publish(DeleteNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Publish(newsDto);

                    return Ok();
                });
        }

        [Route("PublishArray")]
        public async Task<IActionResult> PublishArray(List<int> ids)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.PublishArray(ids.ToArray());

                return Ok();
            });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete(DeleteNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Delete(newsDto);

                    return Ok();
                });
        }
        [HttpPost, Route("Restore")]
        public async Task<IActionResult> Restore(RestoreNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Restore(newsDto);

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

        [HttpPut]
        [Route("Alert")]
        public async Task<IActionResult> Alert(DeleteNewsDtoBindingModel dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.Alert(dto);

            return Ok();
        }
    }
}
