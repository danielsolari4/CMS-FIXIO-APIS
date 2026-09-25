using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.Dtos.Configuration;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.FrontendApi.Controllers
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
