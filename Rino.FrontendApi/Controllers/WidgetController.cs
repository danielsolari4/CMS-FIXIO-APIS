using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.Dtos.Configuration;
using Rino.Managers;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Widget")]
    public class WidgetController : BaseApiController
    {
        private readonly IWidgetManager _widgetManager;
        private readonly AppSettings _appSettings;

        public WidgetController(IWidgetManager manager, AppSettings appSettings)
        {
            _widgetManager = manager;
            _appSettings = appSettings;
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(SolrCore.WIDGET,
                HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString())), _appSettings.Solr)));
        }


    }
}
