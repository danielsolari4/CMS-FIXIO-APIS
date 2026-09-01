using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.BackendApi.Controllers.ExceptionController;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Exception;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Settings")]
    public class SettingsController : BaseApiController
    {
        private readonly ISettingsManager _manager;

        public SettingsController(ISettingsManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string name)
        {
            return await TryJsonResultAsync(async () =>
            {
                var settings = await _manager.GetAll();
                if (settings == null)
                    return NotFound();

                if (settings.Count > 0 && settings.Where(s => s.Name == name).Any())
                    return Ok(settings.FirstOrDefault(s => s.Name == name));
                return null;
            });

        }

        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeMedia = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take()), pagination));
            });
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SettingsDto settingsDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                if (settingsDto == null) return null;
                var result = await _manager.Add(settingsDto);

                return Ok(CMSResponse(result));
            });
        }


        [HttpPut]
        public async Task<IActionResult> Put([FromBody] SettingsDto settingsDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Update(settingsDto);

                return Ok();
            });
        }

        [HttpDelete, Route("Delete")]
        public async Task<IActionResult> Delete([FromBody] SettingsDto settingsDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Delete(settingsDto);

                return Ok();
            });
        }
    }
}
