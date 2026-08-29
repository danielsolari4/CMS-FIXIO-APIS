using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos.Configuration;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Gallery")]
    public class GalleryController : BaseApiController
    {
        private readonly AppSettings _appSettings;

        public GalleryController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        [HttpGet]
        [Route("GetAllSolr")]
        
        public async Task<IActionResult> GetAllSolr()
        {
            return Ok(await SolrHelper.ExecuteQuery(SolrCore.GALLERY, HttpUtility.UrlDecode(Request.QueryString.ToString()), _appSettings.Solr));
        }
    }
}
