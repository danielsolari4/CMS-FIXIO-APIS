using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using Rino.Dtos.Configuration;

namespace Rino.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/App")]
    public class AppController : BaseApiController
    {
        private readonly AppSettings _appSettings;
        public AppController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }


        [HttpGet, Route("GetAppVersionRequired")]        
        public async Task<IActionResult> GetAppVersionRequired(string platform)
        {
            return await TryJsonResultAsync(async () =>
            {
                switch (platform.ToLower())
                {
                    case "android":
                        {
                            return Ok(new { RequiredAppVersion = _appSettings.MobileApp.VersionRequiredAndroid });
                        }
                    case "ios":
                        {
                            return Ok(new { RequiredAppVersion = _appSettings.MobileApp.VersionRequiredIos });
                        }
                    default:
                        {
                            return Ok(new { RequiredAppVersion = _appSettings.MobileApp.VersionRequiredAndroid });
                        }
                }
            });
        }
    }
}