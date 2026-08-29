using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Service")]
    public class ServiceController : BaseApiController
    {
        [HttpGet]
        [Route("GetCurrentConditions")]
        public async Task<HttpResponseMessage> GetCurrentConditions(string apikey, string language, bool details)
        {
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://dataservice.accuweather.com/currentconditions/v1/7894?apikey=" + apikey + "&language=" + language + "&details=" + details);

            return response;
        }
    }
}
