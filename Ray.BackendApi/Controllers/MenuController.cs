using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.BackendApi.Attributes;
using Ray.BackendApi.Controllers.ExceptionController;
using Ray.Dtos.JsonEntities;
using Ray.Managers;
using Ray.Utils.Exception;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Menu")]
    public class MenuController : BaseApiController
    {
        private readonly IMenuManager _manager;

        public MenuController(IMenuManager manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var node = await _manager.GetById(id);

                    if (node == null)
                        return NotFound();

                    return Ok(CMSResponse(node));
                });
        }

        [HttpGet]
        [Route("GetByTypeId")]
        public async Task<IActionResult> GetByTypeId(int typeId)
        {
            return await TryJsonResultAsync(async () =>
            {
                var node = await _manager.GetByTypeId(typeId);

                if (node == null)
                    return NotFound();

                return Ok(CMSResponse(node));
            });
        }

        [HttpGet]
        [Route("GetTypes")]
        public IActionResult GetTypes()
        {
            return TryAction(() =>
            {
                return Ok(CMSResponse(_manager.GetTypes()));
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]MenuJson menuDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var result = await _manager.Add(menuDto);

                    return Ok(CMSResponse(result));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put(MenuJson menuDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(menuDto);

                    return Ok();
                });
        }

        [HttpDelete]
        [HttpPost, Route("Delete")]
        public async Task<IActionResult> Delete(MenuJson menuDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());
                    
                    await _manager.Delete(menuDto);

                    return Ok();
                });
        }
    }
}
