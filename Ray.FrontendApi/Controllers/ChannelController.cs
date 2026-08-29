using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
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

        [HttpGet]
        [Route("GetChannels")]        
        
        public async Task<IActionResult> GetChannels()
        {
            return Ok(await SolrHelper.ExecuteQuery(SolrCore.CHANNEL, HttpUtility.UrlDecode(Request.QueryString.ToString()), _appSettings.Solr));
        }

        #endregion
    }
}