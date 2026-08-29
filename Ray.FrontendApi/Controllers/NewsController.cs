using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using CMS.FrontendAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.FrontendApi.Controllers.ExceptionController;
using Ray.Managers;
using Ray.Utils.Exception;
using Ray.Utils.Logging;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/News")]
    public class NewsController : BaseApiController
    {
        private readonly INewsManager _manager;
        private readonly IUserManager _userManager;
        private readonly AppSettings _appSettings;
        private readonly IStatsManager _statsManager;

        public NewsController(INewsManager manager, IUserManager userManager, AppSettings appSettings, IStatsManager statsManager)
        {
            _manager = manager;
            _userManager = userManager;
            _appSettings = appSettings;
            _statsManager = statsManager;
        }

        
       public async Task<IActionResult> Get(int id, bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                var news = await _manager.GetById(id, includeContent, includeAuthors, includeNodes, includeMedia, includeGalleries, includeRelatedAssets, languageId);

                if (news == null)
                    return NotFound();

                return Ok(CMSResponse(news));
            });
        }

        
       public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(await _manager.GetAll(includeContent, includeAuthors, includeNodes, includeMedia, includeGalleries, includeRelatedAssets, languageId, pagination.Skip(), pagination.Take()), pagination));
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.NEWS, HttpUtility.UrlDecode(Request.QueryString.ToString()), _appSettings.Solr));
            });
        }


        [HttpGet]
        [Route("GetMostRead")]
        public async Task<IActionResult> GetMostRead(int nodeId)
        {
            string MostReadList = string.Empty;
            if (nodeId == 0)
                MostReadList = _statsManager.GetMostRead(10);
            if (string.IsNullOrEmpty(MostReadList))
            {
                MostReadList = await _statsManager.GetMostReadOldAsync(10, nodeId);
            }

            if (!String.IsNullOrEmpty(MostReadList))
            {
                var response = await SolrHelper.ExecuteQuery(SolrCore.NEWS,
                    HttpUtility.UrlDecode("q=Id:(" + MostReadList + ") AND IsDeleted:false AND IsPrivate:false AND Status:PUBLISHED&rows=6&fl=Id, Url,Title_en,Description_en,Nodes_en,Nodes_slug,MediaSizesPaths:[json],PublicationDate"),
                    _appSettings.Solr);

                if (!string.IsNullOrWhiteSpace(response.ToString()))
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(response.ToString());


                    if (result != null && result.response.docs != null)
                    {
                        var resp = result.response.docs;
                        return Ok(resp);
                    }
                }
            }
            return Ok(JsonConvert.DeserializeObject<dynamic>(""));

        }

        [HttpPost]
        [Route("ViewsCount")]
        public async Task<IActionResult> ViewsCount(int id, int nodeId, DateTime publicationDate, string slug)
        {
            await _statsManager.ViewCount(id, nodeId, publicationDate, slug);
            return Ok();
        }

        //[HttpPut]
        //[Route("ViewsCount")]
        //
        //public async Task<IActionResult> ViewsCount(CountDto dto)
        //{
        //    return await TryJsonResultAsync(async () =>
        //    {
        //        if (!ModelState.IsValid)
        //            throw new ModelException(ModelState.GetErrorMessage());



        //        dto.Discriminator = CountDiscriminator.Views;
        //        await _manager.UpdateCounts(dto);

        //        return Ok();
        //    });
        //}

        [HttpPut]
        [Route("ShareCount")]
        public async Task<IActionResult> ShareCount(CountDto dto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                dto.Discriminator = CountDiscriminator.Share;
                await _manager.UpdateCounts(dto);

                return Ok();
            });
        }

        [HttpGet]
        [Route("GetSiteMapNews")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSitemapNews()
        {
            try
            {
                SitemapHelper.ValidateDirectory(_appSettings.Sitemap.FolderPath);
                var pathFile = System.IO.Path.Combine(_appSettings.Sitemap.FolderPath, "sitemaps", "sitemap-sections.xml");

                var news = await SolrHelper.ExecuteQuery(SolrCore.NODE, "fl=Id,Description,LastModificationDate&indent=on&q=IsPublished%3Atrue%20AND%0AIsDeleted%3Afalse&rows=2147483647&sort=Description%20asc", _appSettings.Solr);
                var token = JObject.Parse(news.ToString());
                var childs = (dynamic)token.SelectToken("response");
                var tokenChilds = JObject.Parse(childs.ToString());
                var childDocs = ((JProperty)((JContainer)tokenChilds).Last).Value.ToList();

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };

                using (var writer = XmlWriter.Create(pathFile, settings))
                {
                    writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    foreach (var newItem in childDocs.ToArray())
                    {
                        writer.WriteStartElement("url");
                        var description = newItem.SelectToken("Description") != null ? ((JValue)newItem.SelectToken("Description")).Value.ToString() : string.Empty;
                        var lastMod = newItem.SelectToken("LastModificationDate") != null ? ((JValue)newItem.SelectToken("LastModificationDate")).Value.ToString() : DateTime.Now.ToString();
                        var url = "/" + Ray.Dtos.Components.ComponentNewDto.GenerateSlug(description);
                        SitemapHelper.PriorityFreq(url, out var changefreq, out var priority);
                        writer.WriteElementString("loc", $"{SitemapHelper.GetFrontUrl(_appSettings.Content.FrontendUrl ?? string.Empty)}{url}");
                        writer.WriteElementString("lastmod", DateTime.Parse(lastMod).ToString("yyyy-MM-ddTHH:mm:ssssZ"));
                        writer.WriteElementString("changefreq", changefreq);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.Flush();
                }

                return PhysicalFile(pathFile, "application/xml");
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("GetCurrentSitemapNews")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentSitemapNews()
        {
            try
            {
                SitemapHelper.ValidateDirectory(_appSettings.Sitemap.FolderPath);
                var rangeDate = await SitemapHelper.GetRangeDate(_appSettings.Solr);
                SitemapHelper.WriteGenericSitemap(rangeDate, _appSettings.Sitemap.FolderPath, _appSettings.Content.FrontendUrl ?? string.Empty);
                var pathFile = await SitemapHelper.GetNewsByMonth(DateTime.Now.Month, DateTime.Now.Year, _appSettings.Solr, _appSettings.Sitemap.FolderPath, _appSettings.Content.FrontendUrl ?? string.Empty);

                return PhysicalFile(pathFile, "application/xml");
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("GetFullSitemap")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFullSitemap()
        {
            try
            {
                SitemapHelper.ValidateDirectory(_appSettings.Sitemap.FolderPath);
                var rangeDate = await SitemapHelper.GetRangeDate(_appSettings.Solr);
                var newRange = (FilterSitemap)rangeDate.Clone();

                SitemapHelper.WriteGenericSitemap(rangeDate, _appSettings.Sitemap.FolderPath, _appSettings.Content.FrontendUrl ?? string.Empty);

                while (newRange.StartDate.Month != newRange.EndDate.Month || newRange.StartDate.Year != newRange.EndDate.Year)
                {
                    await SitemapHelper.GetNewsByMonth(newRange.StartDate.Month, newRange.StartDate.Year, _appSettings.Solr, _appSettings.Sitemap.FolderPath, _appSettings.Content.FrontendUrl ?? string.Empty);
                    newRange.StartDate = newRange.StartDate.AddMonths(1);
                }

                await GetSitemapNews();
                var pathFile = System.IO.Path.Combine(_appSettings.Sitemap.FolderPath, "sitemap.xml");
                return PhysicalFile(pathFile, "application/xml");
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("GetSiteMapNewsGoogle")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSiteMapNewsGoogle()
        {
            try
            {
                SitemapHelper.ValidateDirectory(_appSettings.Sitemap.FolderPath);
                var pathFile = System.IO.Path.Combine(_appSettings.Sitemap.FolderPath, "sitemap-google-news.xml");
                var news = await SolrHelper.ExecuteQuery(SolrCore.NEWS, "fl=Id,Url,LastModificationDate,PublicationDate,Title_en,Keywords_en,Nodes_en&indent=on&q=IsDeleted:false%20AND%20Status:PUBLISHED%20AND%20PublicationDate:[NOW-2DAY TO NOW]&rows=1000&sort=PublicationDate%20desc", _appSettings.Solr);
                var token = JObject.Parse(news.ToString());
                var childs = (dynamic)token.SelectToken("response");
                var tokenChilds = JObject.Parse(childs.ToString());
                var childDocs = ((JProperty)((JContainer)tokenChilds).Last).Value.ToList();

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };

                using (var writer = XmlWriter.Create(pathFile, settings))
                {
                    writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    writer.WriteAttributeString("xmlns", "news", null, "http://www.google.com/schemas/sitemap-news/0.9");

                    foreach (var newItem in childDocs.ToArray())
                        SitemapHelper.ParseCompleteXmlItem(newItem, writer, SitemapHelper.GetFrontUrl(_appSettings.Content.FrontendUrl ?? string.Empty), string.Empty, string.Empty);

                    writer.WriteEndElement();
                    writer.Flush();
                }

                return PhysicalFile(pathFile, "application/xml");
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("GetSiteMapVideos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSiteMapVideos()
        {
            SitemapHelper.ValidateDirectory(_appSettings.Sitemap.FolderPath);
            try
            {
                var pathFile = System.IO.Path.Combine(_appSettings.Sitemap.FolderPath, "sitemap-videos.xml");
                var news = await SolrHelper.ExecuteQuery(SolrCore.MEDIA, "fl=Id,MediaUrl,LastModificationDate,PublicationDate,Title,SourcePath,Description&indent=on&q=IsEnabled:true%20AND%20Discriminator:video&rows=100&sort=PublicationDate%20desc", _appSettings.Solr);
                var token = JObject.Parse(news.ToString());
                var childs = (dynamic)token.SelectToken("response");
                var tokenChilds = JObject.Parse(childs.ToString());
                var childDocs = ((JProperty)((JContainer)tokenChilds).Last).Value.ToList();

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };

                using (var writer = XmlWriter.Create(pathFile, settings))
                {
                    writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    writer.WriteAttributeString("xmlns", "video", null, "http://www.google.com/schemas/sitemap-video/1.1");

                    foreach (var newItem in childDocs.ToArray())
                    {
                        writer.WriteStartElement("url");
                        writer.WriteElementString("loc", $"<![CDATA[ {_appSettings.Content.FrontendUrl}Multimedia/Videos/{((JValue)newItem.SelectToken("Id")).Value} ]]>");
                        writer.WriteStartElement("video", "video", null);
                        writer.WriteElementString("video", "content_loc", null, newItem.SelectToken("MediaUrl") != null ? $"<![CDATA[ {((JValue)newItem.SelectToken("MediaUrl")).Value} ]]>" : null);
                        writer.WriteElementString("video", "duration", null);
                        writer.WriteElementString("video", "thumbnail_loc", null, newItem.SelectToken("SourcePath") != null ? $"{_appSettings.Content.ImageUrl}{((JValue)newItem.SelectToken("SourcePath")).Value}" : null);
                        writer.WriteElementString("video", "title", null, newItem.SelectToken("Title") != null ? ((JValue)newItem.SelectToken("Title")).Value.ToString() : null);
                        writer.WriteElementString("video", "description", null, newItem.SelectToken("Description") != null ? ((JValue)newItem.SelectToken("Description")).Value.ToString() : null);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.Flush();
                }

                return PhysicalFile(pathFile, "application/xml");
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return BadRequest();
            }
        }



        //[HttpGet]
        //[Route("GetSiteMapVideos")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetSiteMapVideos()
        //{
        //    SitemapHelper.ValidateDirectory(_appSettings.Sitemap.FolderPath);

        //    try
        //    {
        //        //TODO: ver comentado
        //        var pathFile = _appSettings.Sitemap.FolderPath + "sitemap-videos.xml";

        //        var news = await SolrHelper.ExecuteQuery(SolrCore.MEDIA, "fl=Id,MediaUrl,LastModificationDate,PublicationDate,Title,SourcePath,Description&indent=on&q=IsEnabled:true%20AND%20Discriminator:video&rows=100&sort=PublicationDate%20desc", _appSettings.Solr);
        //        var token = JObject.Parse(news.ToString());
        //        var childs = (dynamic)token.SelectToken("response");
        //        var tokenChilds = JObject.Parse(childs.ToString());
        //        var childDocs = ((JProperty)((JContainer)tokenChilds).Last).Value.ToList();

        //        #region xml settings configuration

        //        XmlWriterSettings settings = new XmlWriterSettings
        //        {
        //            Indent = true,
        //            IndentChars = "  ",
        //            NewLineChars = "\r\n",
        //            NewLineHandling = NewLineHandling.Replace
        //        };

        //        #endregion

        //        #region build xml file

        //        using (var writer = XmlWriter.Create(pathFile, settings))
        //        {
        //            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

        //            writer.WriteAttributeString("xmlns", "video", null, "http://www.google.com/schemas/sitemap-video/1.1");

        //            foreach (var newItem in childDocs.ToArray())
        //            {
        //                #region Url node

        //                writer.WriteStartElement("url");

        //                writer.WriteElementString("loc", $"<![CDATA[ {_appSettings.Content.FrontendUrl}Multimedia/Videos/{((JValue)newItem.SelectToken("Id")).Value.ToString()} ]]>");

        //                #region Video node

        //                writer.WriteStartElement("video", "video", null);

        //                //MediaUrl
        //                if (newItem.SelectToken("MediaUrl") != null)
        //                    writer.WriteElementString("video", "content_loc", null, $"<![CDATA[ {((JValue)newItem.SelectToken("MediaUrl")).Value.ToString()} ]]>");
        //                else
        //                    writer.WriteElementString("video", "content_loc", null);

        //                writer.WriteElementString("video", "duration", null);

        //                //Thumbnail
        //                if (newItem.SelectToken("SourcePath") != null)
        //                    writer.WriteElementString("video", "thumbnail_loc", null, $"{_appSettings.Content.ImageUrl}{((JValue)newItem.SelectToken("SourcePath")).Value.ToString()}");
        //                else
        //                    writer.WriteElementString("video", "thumbnail_loc", null);

        //                //Title
        //                if (newItem.SelectToken("Title") != null)
        //                    writer.WriteElementString("video", "title", null, ((JValue)newItem.SelectToken("Title")).Value.ToString());
        //                else
        //                    writer.WriteElementString("video", "title", null);

        //                //Description
        //                if (newItem.SelectToken("Description") != null)
        //                    writer.WriteElementString("video", "description", null, ((JValue)newItem.SelectToken("Description")).Value.ToString());
        //                else
        //                    writer.WriteElementString("video", "description", null);

        //                //PublicationDate
        //                if (newItem.SelectToken("PublicationDate") != null)
        //                    writer.WriteElementString("video", "publication_date", null, DateTime.Parse(((JValue)newItem.SelectToken("PublicationDate")).Value.ToString()).ToString("yyyy-MM-ddTHH:mm:ssssZ"));
        //                else
        //                    writer.WriteElementString("video", "publication_date", null);

        //                writer.WriteEndElement();

        //                #endregion

        //                //LastModificationDate
        //                if (newItem.SelectToken("LastModificationDate") != null)
        //                    writer.WriteElementString("lastmod", DateTime.Parse(((JValue)newItem.SelectToken("LastModificationDate")).Value.ToString()).ToString("yyyy-MM-ddTHH:mm:ssssZ"));
        //                else
        //                    writer.WriteElementString("lastmod", null);

        //                writer.WriteEndElement();
        //                writer.Flush();

        //                #endregion
        //            }

        //            writer.WriteEndElement();
        //            writer.Flush();
        //        }

        //        #endregion

        //        #region return xml to browser

        //        //TODO:ver comentado
        //        //HttpContext.Current.Response.Clear();
        //        //HttpContext.Current.Response.Buffer = true;
        //        //HttpContext.Current.Response.Charset = "";
        //        //HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //        //HttpContext.Current.Response.ContentType = "application/xml";
        //        //HttpContext.Current.Response.WriteFile(pathFile);
        //        //HttpContext.Current.Response.Flush();
        //        //HttpContext.Current.Response.End();

        //        #endregion

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        CMSLogger.Error(ex.Message);
        //        return BadRequest();
        //    }
        //}
    }
}

