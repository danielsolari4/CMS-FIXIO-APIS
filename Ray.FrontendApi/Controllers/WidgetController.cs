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
