using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.BackendApi.Attributes;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Channel")]
    public class ChannelController : BaseApiController
    {
        private readonly IChannelManager _channelManager;
        private readonly AppSettings _appSettings;

        public ChannelController(IChannelManager channelManager, AppSettings appSettings)
        {
            _channelManager = channelManager;
            _appSettings = appSettings;
        }

        #region Channels

        [HttpGet()]
        //[Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var channel = await _channelManager.GetById(id);

                if (channel == null)
                    return NotFound();

                return Ok(CMSResponse(channel));
            });
        }

        [HttpGet("GetChannels")]
        [Route("GetChannels")]
        public async Task<IActionResult> GetChannels()
        {
            return Ok(await SolrHelper.ExecuteQuery(SolrCore.CHANNEL, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr));
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> Post(ChannelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.CreationDate = DateTime.UtcNow;
            dto.CreationUser = GetLoggedUser().Id.ToString();
            var result = await _channelManager.Add(dto);

            return Ok(CMSResponse(result));
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> Put(ChannelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.LastModificationDate = DateTime.UtcNow;
            dto.LastModificationUser = GetLoggedUser().Id.ToString();

            await _channelManager.Update(dto);

            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(ChannelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _channelManager.Delete(dto);

            return Ok();
        }

        #endregion
    }
}