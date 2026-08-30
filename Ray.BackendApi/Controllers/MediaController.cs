using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Ray.BackendApi.Attributes;
using Ray.BackendApi.Controllers.ExceptionController;
using Ray.BackendApi.Models;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Backload;
using Ray.Utils.Configuration;
using Ray.Utils.Exception;
using Ray.Utils.Helpers;
using Ray.Utils.Solr;
using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Media")]
    public class MediaController : BaseApiController
    {
        private readonly IMediaManager _manager;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly bool MustUploadToCloud = true;
        private readonly IAmazonS3Manager _amazonS3Manager;

        public MediaController(IMediaManager manager, AppSettings appSettings, IHttpContextAccessor httpContextAccessor, IAmazonS3Manager amazonsS3Manager)
        {
            _manager = manager;
            _appSettings = appSettings;
            _httpContextAccessor = httpContextAccessor;
            _amazonS3Manager = amazonsS3Manager;
        }

        //[HttpGet]
        [HttpGet]
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

        [HttpGet, Route("GetUsedMediaIds")]
        public async Task<IActionResult> GetUsedMediaIds([FromQuery] List<int> mediaIds)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (mediaIds == null || !mediaIds.Any())
                    return LegacyBadRequest("No media IDs provided.");

                var usedMediaIds = await _manager.GetUsedMediaIdsAsync(mediaIds);
                return Ok(CMSResponse(usedMediaIds));
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
            return await TryJsonResultAsync(async () => Ok(await SolrHelper.ExecuteQuery(Utils.Solr.SolrCore.MEDIA, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr)));
        }

        [HttpPut]
        public async Task<IActionResult> Put(UpdateMediaDtoBindingModel mediaDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(mediaDto);

                    return Ok();
                });
        }

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

        [HttpPut]
        [Route("ViewsCount")]
        public async Task<IActionResult> ViewsCount(CountDto dto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    dto.Discriminator = CountDiscriminator.Views;
                    await _manager.UpdateCounts(dto);

                    return Ok();
                });
        }


        //TODO: ver codigo comentado

        [HttpPost]
        [Route("BackloadUpdate")]
        public async Task<IActionResult> BackloadUpdate(BackloadUpdateBindingModel model)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (model.MediaId <= 0)
                        throw new ArgumentNullException("id");

                    if (model.Width <= 0)
                        throw new ArgumentNullException("width");

                    if (model.Height <= 0)
                        throw new ArgumentNullException("height");

                    var media = await _manager.GetById(model.MediaId);

                    if (media == null)
                        throw new ArgumentNullException("media");

                    if (!string.IsNullOrWhiteSpace(model.Base64Image))
                    {
                        var replacementPath = ImageStoreHelper.GetMediaSizePath(media, model.Width, model.Height, _appSettings.Media);

                        if (string.IsNullOrWhiteSpace(replacementPath))
                            throw new ArgumentNullException("replacementPath");

                        var base64Image = model.Base64Image;
                        var dataSeparatorIndex = base64Image.IndexOf(',');
                        if (dataSeparatorIndex >= 0)
                            base64Image = base64Image.Substring(dataSeparatorIndex + 1);

                        var bytes = Convert.FromBase64String(base64Image);
                        using var inputStream = new MemoryStream(bytes);
                        using var image = Image.Load(inputStream);
                        using var resizedImage = ImageStoreHelper.CropFixedSize(image, model.Width, model.Height);

                        if (MustUploadToCloud)
                        {
                            await using var outputStream = new MemoryStream();
                            SaveImage(resizedImage, outputStream, replacementPath);
                            outputStream.Position = 0;

                            var uploadResult = await _amazonS3Manager.UploadOneFromStreamAsync(replacementPath, outputStream, GetContentType(replacementPath));

                            if (uploadResult.Error)
                                throw new Exception(uploadResult.Message ?? "AmazonS3 upload failed.");

                            await _manager.Update(media);
                            return Ok();
                        }

                        var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
                        var sizesFolderPath = isLinux ? "" : "/" + _appSettings.Media.Sizes.FolderPath;

                        if (!string.IsNullOrWhiteSpace(replacementPath) && System.IO.File.Exists(string.Format("{0}{1}", sizesFolderPath, replacementPath)))
                        {
                            System.IO.File.Delete((string.Format("{0}{1}", _appSettings.Media.Sizes.FolderPath, replacementPath)));
                            resizedImage.Save(string.Format("{0}{1}", _appSettings.Media.Sizes.FolderPath, replacementPath));

                            if (MustUploadToCloud)
                            {
                                var path = (string.Format("{0}{1}{2}", Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, replacementPath));
                                var uploadResult = await _amazonS3Manager.UploadOneFromFileAsync(replacementPath, path, _appSettings.Media.RemoveAfterUpload);

                                if (uploadResult.Error)
                                    throw new Exception(uploadResult.Message ?? "AmazonS3 upload failed.");
                            }

                           
                            await _manager.Update(media);

                            return Ok();
                        }
                        else
                        {
                            try
                            {
                                var path = string.Format("{0}{1}", _appSettings.Media.Sizes.FolderPath, replacementPath);
                                var directory = Path.GetDirectoryName(isLinux ? path : path.Replace("/", "\\"));
                                if (!string.IsNullOrWhiteSpace(directory))
                                    Directory.CreateDirectory(directory);

                                resizedImage.Save(isLinux ? path : path.Replace("/","\\"));

                                if (MustUploadToCloud)
                                {
                                    var uploadResult = await _amazonS3Manager.UploadOneFromFileAsync(replacementPath, path, _appSettings.Media.RemoveAfterUpload);

                                    if (uploadResult.Error)
                                        throw new Exception(uploadResult.Message ?? "AmazonS3 upload failed.");
                                }

                                await _manager.Update(media);
                                return Ok("Unable to find the specified file.");
                            }
                            catch (Exception ex)
                            {

                                throw;
                            }
                        }
                    }
                    //}

                    throw new ArgumentNullException("file");
                });
        }

        private static readonly string[] dmValidAuthorities = { "dailymotion.com", "www.dailymotion.com", "dai.ly", "www.dai.ly" };
        private static readonly string[] ytValidAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };
        private static readonly string[] ktValidAuthorities = { "kaltura.com", "www.kaltura.com" };

        private void SaveImage(Image image, Stream stream, string fileName)
        {
            image.Save(stream, GetImageEncoder(fileName));
        }

        private IImageEncoder GetImageEncoder(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension == ".jpg" || extension == ".jpeg")
                return new JpegEncoder { Quality = _appSettings.Media.FileUpload.ImageQuality };

            if (extension == ".png")
                return new PngEncoder();

            if (extension == ".bmp")
                return new BmpEncoder();

            if (extension == ".gif")
                return new GifEncoder();

            if (extension == ".tif" || extension == ".tiff")
                return new TiffEncoder();

            throw new Exception("File is not valid format or type.");
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension == ".jpg" || extension == ".jpeg")
                return "image/jpeg";

            if (extension == ".png")
                return "image/png";

            if (extension == ".bmp")
                return "image/bmp";

            if (extension == ".gif")
                return "image/gif";

            if (extension == ".tif" || extension == ".tiff")
                return "image/tiff";

            return "application/octet-stream";
        }

        [HttpPost, Route("Post")]
        public async Task<IActionResult> Post(MediaDto mediaDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (mediaDto.MediaUrl == null)
                        throw new ArgumentNullException("mediaUrl");

                    if (mediaDto.Title == null)
                        throw new ArgumentNullException("title");

                    var builder = new UriBuilder(mediaDto.MediaUrl);

                    var postURL = $"{_appSettings.Content.ApiBackendUrl}api/Backload";
                    var token = HttpContext.Request.Headers["Authorization"];
                    var postParameters = new Dictionary<string, object>();
                    postParameters.Add("Featured", mediaDto.Featured);
                    postParameters.Add("Title", mediaDto.Title);
                    postParameters.Add("Caption", mediaDto.Caption);
                    postParameters.Add("Description", mediaDto.Description);
                    postParameters.Add("AlternativeText", mediaDto.AlternativeText);
                    postParameters.Add("Keywords", mediaDto.Keywords);
                    postParameters.Add("PublicationDate", mediaDto.PublicationDate);
                    postParameters.Add("MediaType", mediaDto.MediaType);

                    foreach (var g in HttpContext.Request.Query.Where(g => g.Key.Contains("Gallery")))
                    {
                        postParameters.Add(g.Key, g.Value);
                    }
                    foreach (var c in HttpContext.Request.Query.Where(c => c.Key.Contains("Category")))
                    {
                        postParameters.Add(c.Key, c.Value);
                    }

                    var imageUrl = string.Empty;

                    if (ytValidAuthorities.Contains(builder.Uri.Authority.ToLower()))
                    {
                        var ytId = FormUpload.ExtractYoutubeVideoIdFromUri(builder.Uri);

                        if (!string.IsNullOrWhiteSpace(ytId))
                        {
                            var fullResImageUrl = string.Format("{0}/vi/{1}/maxresdefault.jpg", _appSettings.Media.Youtube.ImageUrl, ytId);
                            var lowResImageUrl = string.Format("{0}/vi/{1}/0.jpg", _appSettings.Media.Youtube.ImageUrl, ytId);

                            var data = FormUpload.DownloadData(fullResImageUrl);

                            if (data == null)
                                data = FormUpload.DownloadData(lowResImageUrl);

                            if (data != null)
                            {
                                postParameters.Add("MediaUrl", string.Format(_appSettings.Media.Youtube.EmbedUrl, ytId));
                                postParameters.Add("Image[]", new FormUpload.FileParameter(data, mediaDto.Title + ".jpg", "image/jpeg"));
                            }
                        }
                    }
                    else if (dmValidAuthorities.Contains(builder.Uri.Authority.ToLower()))
                    {
                        var dmId = FormUpload.ExtractDailyMotionVideoIdFromUri(builder.Uri);

                        if (!string.IsNullOrWhiteSpace(dmId))
                        {
                            dmId = dmId.Replace("embedvideo", "");
                            var address = string.Format(_appSettings.Media.DailyMotion.ImageUrl, dmId);
                            using (var client = new WebClient() { Encoding = Encoding.UTF8 })
                            {
                                try
                                {
                                    var response = await Task.FromResult(JsonConvert.DeserializeObject<dynamic>(client.DownloadString(new Uri(address))));
                                    imageUrl = response.thumbnail_1080_url;
                                }
                                catch (Exception)
                                { }
                            }

                            var data = FormUpload.DownloadData(imageUrl);

                            if (data != null)
                            {
                                postParameters.Add("MediaUrl", string.Format(_appSettings.Media.DailyMotion.EmbedUrl, dmId));
                                postParameters.Add("Image[]", new FormUpload.FileParameter(data, mediaDto.Title + ".jpg", "image/jpeg"));
                            }
                        }
                    }
                    else if (ktValidAuthorities.Contains(builder.Uri.Authority.ToLower()))
                    {
                        Uri uriString;
                        var webRequest = (HttpWebRequest)WebRequest.Create(mediaDto.MediaUrl);
                        webRequest.AllowAutoRedirect = true;
                        webRequest.Timeout = 10000;
                        webRequest.Method = "HEAD";

                        using (var redirectWebResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            uriString = redirectWebResponse.ResponseUri;
                            redirectWebResponse.Close();
                        }

                        if (uriString != null)
                        {
                            var ktEntryId = FormUpload.ExtractKalturaEntryIdFromUri(uriString);
                            var ktPartnerId = FormUpload.ExtractKalturaPartnerIdFromUri(uriString);

                            if (ktEntryId != null)
                            {
                                imageUrl = string.Format(_appSettings.Media.Kaltura.ImageUrl, ktPartnerId, ktEntryId);

                                var data = FormUpload.DownloadData(imageUrl);

                                if (data != null)
                                {
                                    postParameters.Add("MediaUrl", string.Format(_appSettings.Media.Kaltura.EmbedUrl, ktEntryId));
                                    postParameters.Add("Image[]", new FormUpload.FileParameter(data, mediaDto.Title + ".jpg", "image/jpeg"));
                                }
                            }
                        }
                    }
                    else if (builder.Uri.Authority.ToLower().Contains("soundcloud"))
                    {
                        postParameters.Add("MediaUrl", mediaDto.MediaUrl);
                        //postParameters.Add("MediaType", mediaDto.MediaType);
                        imageUrl = _appSettings.Media.SoundCloud.ImageUrl;

                        var data = FormUpload.DownloadData(imageUrl);

                        //postParameters.Add("file", "https://i1.sndcdn.com/avatars-000131869186-my9qya-t200x200.jpg");
                        //postParameters.Add("file", imageUrl);
                        postParameters.Add("file", new FormUpload.FileParameter(data, mediaDto.Title + ".jpg", "image/jpeg"));
                    }
                    else if (builder.Uri.Authority.ToLower().Contains("facebook"))
                    {
                        Uri uriString;
                        var webRequest = (HttpWebRequest)WebRequest.Create(mediaDto.MediaUrl);
                        webRequest.AllowAutoRedirect = true;
                        webRequest.Timeout = 10000;
                        webRequest.Method = "HEAD";

                        using (var redirectWebResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            uriString = redirectWebResponse.ResponseUri;
                            redirectWebResponse.Close();
                        }

                        var reg = @"(videos|v)(\/|=)(\d+)(\/|&)?";
                        var regex = new System.Text.RegularExpressions.Regex(reg);
                        var val = regex.Matches(mediaDto.MediaUrl);

                        if (val.Count > 0)
                        {
                            var id = string.Empty;
                            foreach (var item in val)
                            {
                                if (!string.IsNullOrEmpty(id)) continue;
                                if (item.ToString().Contains("video") || item.ToString().Contains("v="))
                                    id = item.ToString().ToLower()
                                    .Replace("/videos/", "")
                                    .Replace("videos/", "")
                                    .Replace("v=", "")
                                    .Replace("/", "");

                            }



                            var secret = _appSettings.Media.Facebook.AppSecret;
                            var tokenFacebook = _appSettings.Media.Facebook.AccessToken;
                            var prof = GenerateFacebookSecretProof(tokenFacebook, secret);

                            using (var client = new System.Net.Http.HttpClient())
                            {
                                var parameters = "fields=thumbnails{is_preferred,uri}";
                                client.BaseAddress = new Uri("https://graph.facebook.com/v8.0/");
                                System.Net.Http.HttpResponseMessage response = client.GetAsync($"{id}?{parameters}&appsecret_proof={prof}&access_token={tokenFacebook}").Result;

                                response.EnsureSuccessStatusCode();
                                var content = response.Content.ReadAsStringAsync().Result;
                                if (!string.IsNullOrEmpty(content))
                                {
                                    var obj = JsonConvert.DeserializeObject<FacebookMediaResponseDto>(content);
                                    if (obj != null && obj.thumbnails != null && obj.thumbnails.data != null &&
                                        obj.thumbnails.data.Count > 0)
                                    {
                                        var media = obj.thumbnails.data.FirstOrDefault(x => x.is_preferred);
                                        if (media != null)
                                        {
                                            var data = FormUpload.DownloadData(media.uri);
                                            postParameters.Add("MediaUrl", string.Format(_appSettings.Media.Facebook.EmbedUrl, HttpUtility.UrlEncode(mediaDto.MediaUrl)));
                                            postParameters.Add("Image[]", new FormUpload.FileParameter(data, mediaDto.Title + ".jpg", "image/jpeg"));
                                        }
                                        else
                                        {
                                            media = obj.thumbnails.data.FirstOrDefault();
                                            if (media == null) return LegacyBadRequest("content");
                                            var data = FormUpload.DownloadData(media.uri);
                                            postParameters.Add("MediaUrl", string.Format(_appSettings.Media.Facebook.EmbedUrl, HttpUtility.UrlEncode(mediaDto.MediaUrl)));
                                            postParameters.Add("Image[]", new FormUpload.FileParameter(data, mediaDto.Title + ".jpg", "image/jpeg"));
                                        }
                                    }
                                }
                            }

                        }


                        if (uriString != null)
                        {
                            var ktEntryId = FormUpload.ExtractKalturaEntryIdFromUri(uriString);
                            var ktPartnerId = FormUpload.ExtractKalturaPartnerIdFromUri(uriString);

                            if (ktEntryId != null)
                            {
                                imageUrl = string.Format(_appSettings.Media.Kaltura.ImageUrl, ktPartnerId, ktEntryId);

                                var data = FormUpload.DownloadData(imageUrl);

                                if (data != null)
                                {
                                    postParameters.Add("MediaUrl", string.Format(_appSettings.Media.Kaltura.EmbedUrl, ktEntryId));
                                    postParameters.Add("Image[]", new FormUpload.FileParameter(data, mediaDto.Title + ".jpg", "image/jpeg"));
                                }
                            }
                        }
                    }

                    if (postParameters.Any())
                    {
                        var webResponse = FormUpload.MultipartFormDataPost(postURL, postParameters, token);
                        var fullResponse = string.Empty;

                        using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            fullResponse = responseReader.ReadToEnd();
                            webResponse.Close();
                        }

                        return Ok(JsonConvert.DeserializeObject<dynamic>(fullResponse));
                    }

                    throw new Exception("MEEX_001");
                });


        }
        public static string GenerateFacebookSecretProof(string facebookAccessToken, string facebookAuthAppSecret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(facebookAuthAppSecret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(facebookAccessToken);
            HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes);
            byte[] hash = hmacsha256.ComputeHash(messageBytes);
            StringBuilder sbHash = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sbHash.Append(hash[i].ToString("x2"));
            }

            return sbHash.ToString();
        }
    }
}
