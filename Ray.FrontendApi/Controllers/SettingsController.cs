using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Solr;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Settings")]
    public class SettingsController : BaseApiController
    {
        private readonly ISettingsManager _manager;
        private readonly AppSettings _appSettings;
        public SettingsController(ISettingsManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        [HttpGet, Route("Get")]
        public async Task<IActionResult> Get(string name)
        {
            var result = await SolrHelper.ExecuteQuery(Utils.Solr.SolrCore.SETTINGSCORE, HttpUtility.UrlDecode("q=Name:" + name), _appSettings.Solr);
            var stringJson = JsonConvert.SerializeObject(result);

            SolrResponse settings = JsonConvert.DeserializeObject<SolrResponse>(stringJson);

            if (settings == null)
                return NotFound();
            var val = settings?.response?.docs?.Count > 0 ? settings?.response?.docs[0] : null;
            return Ok(val);
        }

        [HttpGet, Route("GetSettings")]
        public Task<IActionResult> GetSettings(string name)
        {
            return Get(name);
        }
    }
}
