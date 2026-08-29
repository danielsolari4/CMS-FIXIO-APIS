using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Dtos.JsonEntities;
using Ray.Managers;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Page")]
    public class PageController : BaseApiController
    {
        private readonly IPageManager _manager;
        private readonly AppSettings _appSettings;

        public PageController(IPageManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                var page = await _manager.GetById(id, includeContent, includeNodes, includeMedia, includeGalleries, languageId);

                if (page == null)
                    return NotFound();

                return Ok(CMSResponse(page));
            });
        }

        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(await _manager.GetAll(includeContent, includeNodes, includeMedia, includeGalleries, languageId, pagination.Skip(), pagination.Take()), pagination));
            });
        }

        [HttpGet]
        [Route("CleanCache")]
        public bool CleanCache(string controller, string action)
        {
            //TODO: ver comentado
            //var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);

            //if (controller == "MenuController")
            //{
            //    cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((MenuController x) => x.GetByTypeId((int)MenuType.Header)));
            //    cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((MenuController x) => x.GetByTypeId((int)MenuType.Footer)));
            //    cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((MenuController x) => x.GetByTypeId((int)MenuType.Sidebar)));
            //}
            //else if (!string.IsNullOrEmpty(controller) && controller.ToLower().Contains("keyword"))
            //{
            //    cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((KeywordController x) => x.GetAllSolr()));
            //    cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((KeywordController x) => x.GetAll(null)));
            //}
            //else if (!string.IsNullOrEmpty(controller) && controller.ToLower().Contains("news"))
            //{
            //    var q = "Id:" + action + " AND IsPrivate:false AND IsDeleted:false AND Status:PUBLISHED AND PublicationDate:[* TO NOW]";
            //    var fl = "Id,Nodes_en,Nodes_slug,AuthorId,Nodes_Id,Title_en,SocialNetworkTitle_en,Volanta_en,AccessType,AssetJson,MediaDiscriminator,MediaUrl,MediaDescription,Description_en,Content_en,PublicationDate,Keywords_en,MediaSizesPaths:[json],PrintEdition:[json],Galleries,Node_Microsite,AuthorName,TemplateId,TemplateClass";

            //    System.Reflection.PropertyInfo isReadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //    isReadonly.SetValue(HttpContext.Current.Request.QueryString, false, null);
            //    HttpContext.Current.Request.QueryString.Clear();
            //    HttpContext.Current.Request.QueryString.Add("q", q);
            //    HttpContext.Current.Request.QueryString.Add("fl", fl);
            //    isReadonly.SetValue(HttpContext.Current.Request.QueryString, true, null);
                
            //    cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((NewsController x) => x.GetAllSolr()));

            //    //en caso de que no funcione probar este clean
            //    //var cacheKey = cache.AllKeys.Where(x => x.ToLower().Contains(("Id:" + action).ToLower()));
            //    //foreach (var item in cacheKey)
            //    //{
            //    //    if (!string.IsNullOrEmpty(item))
            //    //        cache.Remove(item);
            //    //}
            //}
            //else
            //    cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((LayoutInstanceController x) => x.GetSolrByNodeId(int.Parse(action))));

            //if (cache.Contains(controller + "/" + action))
            //    cache.Remove(controller + "/" + action);

            return true;
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.PAGE, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr));
            });
        }
        //[HttpGet]
        //[Route("GetAllSolr")]
        //public async Task<IActionResult> GetAllSolr()
        //{
        //    return Ok(await SolrHelper.ExecuteQuery(SolrCore.PAGE, HttpUtility.UrlDecode(HttpContext.Current.Request.QueryString.ToString())));
        //}
    }
}
