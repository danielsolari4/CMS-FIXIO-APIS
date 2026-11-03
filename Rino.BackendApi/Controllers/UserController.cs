using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly IApplicationUserManager _applicationUserManager;
        public UserController(IUserManager manager, AppSettings appSettings, IHttpContextAccessor httpContextAccessor, IAmazonS3Manager amazonS3Manager, IApplicationUserManager applicationUserManager)
        {
            _manager = manager;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
            _amazonS3Manager = amazonS3Manager;
            _applicationUserManager = applicationUserManager;
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

        [HttpGet, Route("GetLockedOut")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetLockedOut()
        {
            if (!await HasAnyUserPermission("GetAllSolr", "GetAll", "Get"))
                return Forbid(JwtBearerDefaults.AuthenticationScheme);

            return await TryJsonResultAsync(async () =>
                Ok(CMSResponse(await _manager.GetLockedOut())));
        }

        [HttpPut, Route("Unlock")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Unlock(UpdateUserDtoBindingModel userDto)
        {
            if (!await HasAnyUserPermission("Put"))
                return Forbid(JwtBearerDefaults.AuthenticationScheme);

            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Unlock(userDto.Id);
                return Ok();
            });
        }

        private async Task<bool> HasAnyUserPermission(params string[] actionNames)
        {
            var loggedUser = GetLoggedUser();
            if (loggedUser == null)
                return false;

            var account = await _applicationUserManager.FindByIdAsync(loggedUser.Id.ToString(), System.Threading.CancellationToken.None);
            if (account == null || !account.IsEnabled || account.IsDeleted)
                return false;

            var actionClaim = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData)?.Value;
            if (string.IsNullOrWhiteSpace(actionClaim))
                return false;

            var actionIds = JsonConvert.DeserializeObject<System.Collections.Generic.List<int>>(actionClaim);
            if (actionIds == null)
                return false;

            foreach (var actionName in actionNames)
            {
                if (await _applicationUserManager.HasAccessToActionAsync("User", actionName, actionIds))
                    return true;
            }

            return false;
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

                    if (formData.Files.Count < 1)
                    {
                        ModelState.AddModelError("Image", "IMEX_001");
                        throw new ModelException(ModelState.GetErrorMessage());
                    }

                    var file = formData.Files[0];
                    var user = await _manager.GetById(GetLoggedUser().Id);
                    var oldProfileImagePath = user.ProfileImagePath;
                    user.ProfileImagePath = await UploadProfileImageToCloud(oldProfileImagePath, file);
                    await _manager.SetProfileImagePath(user);
                    await TryDeleteOldProfileImage(oldProfileImagePath);
                    return Ok(user.ProfileImagePath);
                });
        }


        private async Task<string> UploadProfileImageToCloud(string oldProfileImagePath, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("Image", "IMEX_001");
                throw new ModelException(ModelState.GetErrorMessage());
            }

            if (string.IsNullOrWhiteSpace(_appSettings.AmazonS3?.BucketName))
            {
                ModelState.AddModelError("Image", "IMEX_002");
                throw new ModelException(ModelState.GetErrorMessage());
            }

            ImageStoreHelper.ValidateProfileImage(file.FileName, file.ContentType, file.Length, _appSettings.Media);

            var profileImagePath = ImageStoreHelper.CreateProfileImageCloudPath(file.FileName);
            using var stream = file.OpenReadStream();
            var result = await _amazonS3Manager.UploadOneFromStreamAsync(profileImagePath, stream, file.ContentType);
            if (result.Error)
            {
                ModelState.AddModelError("Image", result.Message ?? "IMEX_003");
                throw new ModelException(ModelState.GetErrorMessage());
            }

            return profileImagePath;
        }

        private async Task TryDeleteOldProfileImage(string oldProfileImagePath)
        {
            if (string.IsNullOrWhiteSpace(oldProfileImagePath))
                return;

            try
            {
                await _amazonS3Manager.DeleteOneAsync(oldProfileImagePath);
            }
            catch
            {
                // A stale or legacy profile image path must not block saving the new upload.
            }
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
