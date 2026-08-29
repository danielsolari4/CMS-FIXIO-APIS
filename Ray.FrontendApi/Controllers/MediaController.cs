using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/Media")]
    public class MediaController : BaseApiController
    {
        private readonly IMediaManager _manager;
        private readonly AppSettings _appSettings;

        public MediaController(IMediaManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

        
        public async Task<IActionResult> Get(int id, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                var media = await _manager.GetById(id, includeGalleries, includeCategories, includeAssets);

                if (media == null)
                    return NotFound();

                return Ok(CMSResponse(media));
            });
        }

        [HttpGet, Route("GetByIds")]
        
        public async Task<IActionResult> GetByIds(int[] mediaIds, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                var mediaList = new List<MediaDto>();

                if (mediaIds != null && mediaIds.Any())
                {
                    foreach (var id in mediaIds)
                    {
                        var media = await _manager.GetById(id, includeGalleries, includeCategories, includeAssets);

                        if (media != null)
                            mediaList.Add(media);
                    }
                }

                return Ok(CMSResponse(mediaList));
            });
        }

        [HttpGet, Route("GetAll")]
        
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take(), includeGalleries, includeCategories, includeAssets), pagination));
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]        
        
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(SolrCore.MEDIA, HttpUtility.UrlDecode(Request.QueryString.ToString()), _appSettings.Solr)));
        }

        [HttpGet, Route("GetArchiveYear")]
        public async Task<IActionResult> GetArchiveYear()
        {
            return await TryJsonResultAsync(async () => Ok(new { year = _appSettings.Content.ArchiveYear }));
        }

        private static readonly string[] dmValidAuthorities = { "dailymotion.com", "www.dailymotion.com", "dai.ly", "www.dai.ly" };
        private static readonly string[] ytValidAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };
        private static readonly string[] ktValidAuthorities = { "kaltura.com", "www.kaltura.com" };


    }
}
