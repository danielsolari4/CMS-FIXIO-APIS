using CMS.NewsVersion.Context;
using CMS.NewsVersion.Helpers.Solr;
using CMS.NewsVersion.Model;
using CMS.NewsVersion.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;

namespace CMS.NewsVersion.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly ILogger<NewsController> _logger;
        private readonly NewsVersionContext _newsVersionContext;
        private readonly IOptions<SolrSettings> _solrSettings;
        private readonly IViewCountService _viewCountService;

        public NewsController(
            ILogger<NewsController> logger,
            NewsVersionContext newsVersionContext,
            IOptions<SolrSettings> solrSettings,
            IViewCountService viewCountService)
        {
            _logger = logger;
            _newsVersionContext = newsVersionContext;
            _solrSettings = solrSettings;
            _viewCountService = viewCountService;
        }

        // Post api/news
        [HttpPost]
        public async Task<IActionResult> Post(JObject news)
        {
            if (news == null) return BadRequest();

            var entity = new News();

            if (news.Properties().Where(x => x.Name == "Id").Any())
                entity.NewsId = (int)news.Properties().First(x => x.Name == "Id").Value;

            if (news.Properties().Where(x => x.Name == "LastModificationUser").Any())
                entity.LastModificationUser = (string?)news.Properties().First(x => x.Name == "LastModificationUser").Value;

            //if (news.Properties().Where(x => x.Name == "LastModificationDate").Any())
            //entity.LastModificationDate = (DateTime)news.Properties().First(x => x.Name == "LastModificationDate").Value;
            entity.LastModificationDate = DateTime.UtcNow;

            if (news.Properties().Where(x => x.Name == "Status").Any())
                entity.Status = (string)news.Properties().First(x => x.Name == "Status").Value;

            if (news["Content"] != null && news["Content"].Any() && news["Content"][0] != null && news["Content"][0]["Title"] != null)
                entity.Title = (string)news["Content"][0]["Title"];

            entity.Content = JsonConvert.SerializeObject(news);

            //var newsId = news.Id;
            await _newsVersionContext.News.AddAsync(entity);
            await _newsVersionContext.SaveChangesAsync();

            await SolrHelper.DataImport(_solrSettings.Value);


            return Ok();
        }



        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return Ok(await SolrHelper.ExecuteQuery(_solrSettings.Value, HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()))));
        }

        [HttpPost]
        [Route("ViewsCount")]
        public async Task<IActionResult> ViewsCount(int id, int nodeId, DateTime publicationDate, string slug, int? authorId = null)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(slug))
                return BadRequest();

            await _viewCountService.ViewCount(id, nodeId, publicationDate, slug, authorId);
            return Ok();
        }
    }
}