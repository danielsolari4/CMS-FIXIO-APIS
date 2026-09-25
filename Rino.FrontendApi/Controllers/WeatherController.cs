using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.Dtos.Configuration;
using Rino.Managers;

namespace Rino.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Weather")]
    public class WeatherController : BaseApiController
    {
        private readonly IWeatherManager _weatherManager;

        public WeatherController(IWeatherManager weatherManager)
        {
            _weatherManager = weatherManager;
        }

        [ResponseCache(CacheProfileName = "WeatherCacheProfile", Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "city" })]
        [Route("GetByCity")]
        [HttpGet]
        public async Task<IActionResult> GetByCity(string city)
        {
            return await TryJsonResultAsync(async () =>
            {
                var weatherData = await _weatherManager.GetByCity(city);
                return Ok(weatherData);
            });
        }

        [ResponseCache(CacheProfileName = "WeatherCacheProfile", Location = ResponseCacheLocation.Any)]
        [Route("GetCities")]
        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await _weatherManager.GetAllowedCities());
            });
        }
    }
}
