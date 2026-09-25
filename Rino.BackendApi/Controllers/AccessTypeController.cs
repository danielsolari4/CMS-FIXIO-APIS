using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Rino.Dtos;
using Rino.Managers;
using Rino.BackendApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/AccessType")]
    public class AccessTypeController : BaseApiController
    {
        private readonly IAccessTypeManager _accessTypeManager;

        public AccessTypeController(IAccessTypeManager accessTypeManager)
        {
            _accessTypeManager = accessTypeManager;
        }

        #region AccessTypes

        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var accessType = await _accessTypeManager.GetById(id);

                if (accessType == null)
                    return NotFound();

                return Ok(CMSResponse(accessType));
            });
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return await TryJsonResultAsync(async () =>
            {
                var accessType = await _accessTypeManager.GetAll();

                if (accessType == null)
                    return NotFound();

                return Ok(CMSResponse(accessType));
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(AccessTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.CreationDate = DateTime.UtcNow;
            dto.CreationUser = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _accessTypeManager.Add(dto);

            return Ok(CMSResponse(result));
        }

        [HttpPut]
        public async Task<IActionResult> Put(AccessTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.LastModificationDate = DateTime.UtcNow;
            dto.LastModificationUser = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _accessTypeManager.Update(dto);

            return Ok();
        }

        [HttpDelete]
        [HttpPost, Route("Delete")]
        public async Task<IActionResult> Delete(AccessTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _accessTypeManager.Delete(dto);

            return Ok();
        }

        #endregion
    }
}
