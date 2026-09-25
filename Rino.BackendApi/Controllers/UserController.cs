using System.Linq;
using System.Net;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rino.BackendApi.Attributes;
using Rino.BackendApi.Controllers.ExceptionController;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers;
using Rino.Utils.Exception;
using Rino.Utils.Helpers;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.BackendApi.Controllers
{

    [Route("api/User")]
    public class UserController : BaseApiController
    {
        private readonly IUserManager _manager;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAmazonS3Manager _amazonS3Manager;
        public UserController(IUserManager manager, AppSettings appSettings, IHttpContextAccessor httpContextAccessor, IAmazonS3Manager amazonS3Manager)
        {
            _manager = manager;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
            _amazonS3Manager = amazonS3Manager;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var user = await _manager.GetById(id);

                    if (user == null)
                        return NotFound();

                    return Ok(CMSResponse(user));
                });
        }

        [HttpGet, Route("GetAll")]
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.USER, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put(CreateUserDtoBindingModel userDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(userDto);

                    return Ok();
                });
        }

        [HttpPost]
        [Route("UpdateProfileImage")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> UpdateProfileImage()
        {
            return await TryJsonResultAsync(async () =>
                {
                    var formData = await _httpContextAccessor.HttpContext.Request.ReadFormAsync();

                    if (formData.Files.Count < 1 || !_appSettings.Media.Profile.ContentTypeAllowed.Contains(formData.Files[0].ContentType.Replace("image/", string.Empty)))
                    {
                        ModelState.AddModelError("Image", "IMEX_001");
                        throw new ModelException(ModelState.GetErrorMessage());
                    }

                    var file = formData.Files[0];
                    var user = await _manager.GetById(GetLoggedUser().Id);
                    var oldProfileImagePath = user.ProfileImagePath;
                    user.ProfileImagePath = await ImageStoreHelper.UpdateUserProfileImage(user.ProfileImagePath, file, _appSettings.Media);
                    await UploadProfileImageToCloud(oldProfileImagePath, user.ProfileImagePath);
                    await _manager.SetProfileImagePath(user);
                    return Ok(user.ProfileImagePath);
                });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateFrontProfileImage")]
        public async Task<IActionResult> UpdateFrontProfileImage(int id = 0)
        {
            //TODO: Probar bien esto!!
            return await TryJsonResultAsync(async () =>
            {
                var formData = await _httpContextAccessor.HttpContext.Request.ReadFormAsync();
                var file = formData.Files[0];

                if (file == null)
                {
                    ModelState.AddModelError("Image", "IMEX_001");
                    throw new ModelException(ModelState.GetErrorMessage());
                }

                var userId = id == 0 ? GetLoggedUser().Id : id;
                var user = await _manager.GetById(userId);
                if (user == null)
                    return BadRequest();

                var oldProfileImagePath = user.ProfileImagePath;
                user.ProfileImagePath = await ImageStoreHelper.UpdateUserProfileImage(user.ProfileImagePath, file, _appSettings.Media);
                await UploadProfileImageToCloud(oldProfileImagePath, user.ProfileImagePath);
                await _manager.SetProfileImagePath(user);
                return Ok(user.ProfileImagePath);
            });
        }

        private async Task UploadProfileImageToCloud(string oldProfileImagePath, string profileImagePath)
        {
            if (string.IsNullOrWhiteSpace(_appSettings.AmazonS3?.BucketName) || string.IsNullOrWhiteSpace(profileImagePath))
                return;

            var filePath = $"{_appSettings.Media.Profile.FolderPath}{profileImagePath.Replace("/", Path.DirectorySeparatorChar.ToString())}";
            var result = await _amazonS3Manager.UploadOneFromFileAsync(profileImagePath, filePath, _appSettings.Media.RemoveAfterUpload);
            if (!result.Error && !string.IsNullOrWhiteSpace(oldProfileImagePath))
                await _amazonS3Manager.DeleteOneAsync(oldProfileImagePath);
        }

        [HttpPut]
        [Route("Enable")]
        public async Task<IActionResult> Enable(UpdateUserDtoBindingModel userDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Enable(userDto.Id);

                    return Ok();
                });
        }

        [HttpPut]
        [Route("Disable")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> Disable(UpdateUserDtoBindingModel userDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var currentUserId = GetLoggedUser().Id.ToString();

                    if (currentUserId == userDto.UserName)
                    {
                        ModelState.AddModelError("Model", "USEX_007");
                        throw new ModelException(ModelState.GetErrorMessage());
                    }

                    await _manager.Disable(userDto.Id);

                    return Ok();
                });
        }

        [HttpDelete]
        [HttpDelete, Route("Delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> Delete(UpdateUserDtoBindingModel userDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var currentUserId = GetLoggedUser().Id.ToString();

                    if (currentUserId == userDto.UserName)
                    {
                        ModelState.AddModelError("Model", "USEX_008");
                        throw new ModelException(ModelState.GetErrorMessage());
                    }

                    await _manager.Delete(userDto);

                    return Ok();
                });
        }
    }
}
