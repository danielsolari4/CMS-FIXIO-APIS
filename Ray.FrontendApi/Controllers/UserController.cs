using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;

namespace Ray.FrontendApi.Controllers
{
    [Route("api/User")]
    public class UserController : BaseApiController
    {
        private readonly IFrontEndUserManager _manager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _appSettings;
        private readonly IAmazonS3Manager _amazonS3Manager;
        public UserController(IFrontEndUserManager manager, IHttpContextAccessor httpContextAccessor, AppSettings appSettings, IAmazonS3Manager amazonS3Manager)
        {
            _manager = manager;
            _httpContextAccessor = httpContextAccessor;
            _appSettings = appSettings;
            _amazonS3Manager = amazonS3Manager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        [HttpPut]
        [Route("Put")]
        public async Task<IActionResult> Put(CreateUserDtoBindingModel userDto)
        {
            ModelState.Clear();
            //   userDto.Id = User.Identity.GetUserId<int>();
            userDto.Id = GetLoggedUser().Id;
            //TODO: ver comentado
            //Validate<CreateUserDtoBindingModel>(userDto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.Update(userDto);

            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        [HttpPost, Route("UpdateProfileImage")]
        public async Task<IActionResult> UpdateProfileImage()
        {
            try
            {
                var formData = await _httpContextAccessor.HttpContext.Request.ReadFormAsync();
                if (formData.Files.Count < 1 || !_appSettings.Media.Profile.ContentTypeAllowed.Contains(formData.Files[0].ContentType.Replace("image/", string.Empty)))
                    return BadRequest(HttpStatusCode.UnsupportedMediaType);

                var file = formData.Files[0];
                var user = await _manager.GetById(GetLoggedUser().Id);
                var oldProfileImagePath = user.ProfileImagePath;

                using (Stream s = file.OpenReadStream())
                {
                    user.ProfileImagePath = Utils.Helpers.ImageStoreHelper.UpdateUserProfileImage(user.ProfileImagePath, s, file.FileName, _appSettings.Media);
                }

                await UploadProfileImageToCloud(oldProfileImagePath, user.ProfileImagePath);
                await _manager.SetProfileImagePath(user);
                return Ok(user.ProfileImagePath);

            }
            catch (System.Exception ex)
            {
                return LegacyBadRequest(ex.Message);
            }
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


        //TODO: ver comentado... ver si esto anda bien!!
        [HttpPut]
        [Route("AddNode")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> AddNode(UpdateUserPreferencesDtoBindingModel userPreferencesDto)
        {
            ModelState.Clear();
            userPreferencesDto.Id = GetLoggedUser().Id;
            //userPreferencesDto.Id = User.Identity.GetUserId<int>();
            //Validate<UpdateUserPreferencesDtoBindingModel>(userPreferencesDto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.AddNode(userPreferencesDto);

            return Ok();
        }

        [HttpPut]
        [Route("RemoveNode")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> RemoveNode(UpdateUserPreferencesDtoBindingModel userPreferencesDto)
        {
            ModelState.Clear();
            userPreferencesDto.Id = GetLoggedUser().Id;
            //userPreferencesDto.Id = User.Identity.GetUserId<int>();
            //TODO: ver comentado
            //Validate<UpdateUserPreferencesDtoBindingModel>(userPreferencesDto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.RemoveNode(userPreferencesDto);

            return Ok();
        }

        [HttpPut]
        [Route("RemoveAllNodes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> RemoveAllNodes()
        {
            var dto = new UpdateUserDtoBindingModel();
            dto.Id = GetLoggedUser().Id;
            //dto.Id = User.Identity.GetUserId<int>();
            //TODO: ver comentado
            //Validate<UpdateUserDtoBindingModel>(dto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.RemoveAllNodes(dto.Id);

            return Ok();
        }

        [Route("GetAllNodes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> GetAllNodes()
        {
            var dto = new UpdateUserDtoBindingModel();
            dto.Id = GetLoggedUser().Id;
            //dto.Id = User.Identity.GetUserId<int>();
            //TODO: ver comentado
            //Validate<UpdateUserDtoBindingModel>(dto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(CMSResponse(await _manager.GetAllNodes(dto.Id)));
        }

        [HttpPut]
        [Route("AddBookmark")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> AddBookmark(UpdateUserBookmarksDtoBindingModel userBookmarksDto)
        {
            ModelState.Clear();
            userBookmarksDto.Id = GetLoggedUser().Id;
            //userBookmarksDto.Id = User.Identity.GetUserId<int>();
            //TODO: ver comentado
            //Validate<UpdateUserBookmarksDtoBindingModel>(userBookmarksDto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.AddBookmark(userBookmarksDto);

            return Ok();
        }

        [HttpPut]
        [Route("RemoveBookmark")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> RemoveBookmark(UpdateUserBookmarksDtoBindingModel userBookmarksDto)
        {
            ModelState.Clear();
            userBookmarksDto.Id = GetLoggedUser().Id;
            //userBookmarksDto.Id = User.Identity.GetUserId<int>();
            //TODO: ver comentado
            //Validate<UpdateUserBookmarksDtoBindingModel>(userBookmarksDto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.RemoveBookmark(userBookmarksDto);

            return Ok();
        }

        [HttpPut]
        [Route("RemoveAllBookmarks")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> RemoveAllBookmarks()
        {
            var dto = new UpdateUserDtoBindingModel();
            dto.Id = GetLoggedUser().Id;
            //dto.Id = User.Identity.GetUserId<int>();
            //TODO: ver comentado
            //Validate<UpdateUserDtoBindingModel>(dto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _manager.RemoveAllBookmarks(dto.Id);

            return Ok();
        }

        [Route("GetAllBookmarks")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> GetAllBookmarks()
        {
            var dto = new UpdateUserDtoBindingModel();
            dto.Id = GetLoggedUser().Id;
            //dto.Id = User.Identity.GetUserId<int>();
            //TODO: ver comentado
            //Validate<UpdateUserDtoBindingModel>(dto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(CMSResponse(await _manager.GetAllBookmarks(dto.Id)));
        }

    }
}
