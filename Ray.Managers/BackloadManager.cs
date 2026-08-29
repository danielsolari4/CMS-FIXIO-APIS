using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Utils.Backload;
using Ray.Utils.Extensions;
using Ray.Utils.Helpers;
using Ray.Utils.Logging;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray.Managers
{
        public interface IBackloadManager
        {
            Task<FileUploadResult> SaveFile(IHttpContextAccessor httpContextAccessor, HttpRequest request);
            Task Delete(string fileName);
            MediaDto UploadImageFromUrl(string url, string title, HttpContext context);
        }
    public class BackloadManager : IBackloadManager
    {
        /// <summary>
        /// Los archivos se subirán a la nube también. Faltaría que una vez hecho se borren del server, o los haga directamente por stream... hay que ver bien
        /// qué se decide al final
        /// </summary>
        private const bool MustUploadToCloud = true;
        private readonly IMemoryCache _memoryCache;
        private readonly UploadSessionManager _uploadSessionManager;
        private readonly AppSettings _appSettings;
        private readonly IMediaManager _mediaManager;
        private readonly IAmazonS3Manager _amazonS3Manager;

        public BackloadManager(IMemoryCache memoryCache, AppSettings appSettings, IMediaManager mediaManager, IAmazonS3Manager amazonsS3Manager)
        {
            _memoryCache = memoryCache;
            _uploadSessionManager = new UploadSessionManager(_memoryCache);
            _appSettings = appSettings;
            _mediaManager = mediaManager;
            _amazonS3Manager = amazonsS3Manager;
        }

        public async Task<FileUploadResult> SaveFile(IHttpContextAccessor httpContextAccessor, HttpRequest request)
        {
            FileUploadResult fileUploadResult = new FileUploadResult();

            try
            {
                var formData = await httpContextAccessor.HttpContext.Request.ReadFormAsync();

                if (formData.Files == null || formData.Files.Count < 1)
                    return new FileUploadResult() { Error = true, Message = "No files found" };

                if (!ImageStoreHelper.IsValidFile(formData.Files[0], _appSettings.Media))
                    return new FileUploadResult() { Error = true, Message = "File is not valid format, type or size" };

                var sessionChunk = _uploadSessionManager.GetInitialSessionData(request, formData.Files[0]);

                sessionChunk.FileName = Path.GetFileName(formData.Files[0].FileName);
                sessionChunk.ThumbnailFileName = Path.GetFileName(formData.Files[0].FileName);

                //Only for the first uploaded part
                #region Set filename and thumbnailname
                if (!_uploadSessionManager.UploadSessionExists(sessionChunk.SessionId))
                {
                    string title = formData["Title"];
                    title = title.Replace(" ", "-").ToLower();

                    if (title.Length > 66)
                        title = title.Substring(0, 65);

                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (char c in title)
                        {
                            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '-' || c == '_')
                            {
                                sb.Append(c);
                            }
                        }

                        title = sb.ToString();
                    }
                    catch
                    {
                        return new FileUploadResult() { Error = true, Message = "An exception has occured. Try again please." };
                    }

                    var newFileName = string.Format("{0}{1}{2}", Math.Abs(Environment.TickCount),
                        sessionChunk.FileName.GetChunckedFileExtension(),
                        Path.GetExtension(sessionChunk.FileName.Replace(" ", string.Empty)));
                    newFileName = title + "_" + newFileName;
                    sessionChunk.FileName = sessionChunk.IsChunkedUpload ?
                        newFileName + sessionChunk.GetPartNumber() : newFileName;

                    sessionChunk.ThumbnailFileName = sessionChunk.IsChunkedUpload ?
                        Path.GetFileNameWithoutExtension(newFileName) : sessionChunk.FileName;
                }
                else
                {
                    var previousSessionChunk = _uploadSessionManager.GetById(sessionChunk.SessionId);
                    string originalExtension = Path.GetExtension(sessionChunk.FileName);
                    sessionChunk.CurrentPartNumber++;
                    sessionChunk.FileName = $"{Path.GetFileNameWithoutExtension(previousSessionChunk.FileName)}" + sessionChunk.GetPartNumber();
                }
                #endregion

                var fileChunk = formData.Files[0];
                string imagePath = string.Empty;
                if (fileChunk != null)
                {
                    imagePath = Path.Combine(_appSettings.Media.Sizes.FolderPath, _appSettings.Media.Sizes.OriginalFolderName,
                        ImageStoreHelper.CreateYearMonthDayFolderByPath(Path.Combine(_appSettings.Media.Sizes.FolderPath, _appSettings.Media.Sizes.OriginalFolderName)),
                        sessionChunk.FileName);

                    using (Stream fileStream = new FileStream(imagePath, FileMode.Create))
                        await fileChunk.CopyToAsync(fileStream);

                    sessionChunk = _uploadSessionManager.CreateOrUpdateUploadSession(sessionChunk);
                }

                var stringToReplace = Directory.GetCurrentDirectory();
                //Merge files
                if (_uploadSessionManager.IsFileReadyToMerge(sessionChunk.SessionId) && sessionChunk.IsChunkedUpload)
                {
                    var mergeResult = await MergeChunkFile(sessionChunk);
                    if (mergeResult.Error)
                        return new FileUploadResult() { Error = true };
                    else
                    {
                        var result = await SaveInDbAndCreateSizes(sessionChunk, formData, mergeResult.FilePath);
                        fileUploadResult.ThumbnailUrl = ImageStoreHelper.GenerateThumbnail(mergeResult.FilePath, _appSettings.Media);
                        fileUploadResult.Url = mergeResult.FilePath;
                        fileUploadResult.Size1Path = result.Size1Path;
                        fileUploadResult.Size2Path = result.Size2Path;
                        fileUploadResult.Size3Path = result.Size3Path;
                        fileUploadResult.Size4Path = result.Size4Path;
                        fileUploadResult.Size5Path = result.Size5Path;
                        fileUploadResult.Size6Path = result.Size6Path;
                        fileUploadResult.Id = result.Id;
                        fileUploadResult.Url = mergeResult.FilePath;
                        fileUploadResult.IsChunkingUploads = false;
                        fileUploadResult.UploadFinished = true;
                    }
                }
                else if (!_uploadSessionManager.IsFileReadyToMerge(sessionChunk.SessionId) && sessionChunk.IsChunkedUpload)
                {
                    fileUploadResult.IsChunkingUploads = true;
                    fileUploadResult.UploadFinished = false;
                }

                if (!sessionChunk.IsChunkedUpload)
                {
                    if (ImageStoreHelper.IsImage(Path.GetExtension(imagePath)))
                        fileUploadResult.ThumbnailUrl = ImageStoreHelper.GenerateThumbnail(imagePath, _appSettings.Media);

                    _uploadSessionManager.ClearUploadSession(sessionChunk.SessionId);

                    var result = await SaveInDbAndCreateSizes(sessionChunk, formData, imagePath);
                    fileUploadResult.Id = result.Id;
                    fileUploadResult.Size1Path = result.Size1Path;
                    fileUploadResult.Size2Path = result.Size2Path;
                    fileUploadResult.Size3Path = result.Size3Path;
                    fileUploadResult.Size4Path = result.Size4Path;
                    fileUploadResult.Size5Path = result.Size5Path;
                    fileUploadResult.Size6Path = result.Size6Path;
                    fileUploadResult.Url = imagePath;
                    fileUploadResult.IsChunkingUploads = false;
                    fileUploadResult.UploadFinished = true;
                }

                //Before return anything, I upload the files to the cloud...
                //If this is finally implemented, the fileuplaodresult should return the amazons3 url data with the files.
                if (MustUploadToCloud)
                    await _amazonS3Manager.UploadFromFileAsync(fileUploadResult, _appSettings.Media.RemoveAfterUpload);


                return fileUploadResult;
            }
            catch (Exception ex)
            {
                return new FileUploadResult() { Error = true };
            }
        }

        public async Task Delete(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
                await _mediaManager.Delete(Path.GetFileName(fileName));
        }

        public MediaDto UploadImageFromUrl(string url, string title, HttpContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var mediaDto = new Dtos.MediaDto();

                    if (mediaDto.Title == null)
                        mediaDto.Title = title;

                    var postURL = $"{_appSettings.Content.ApiBackendUrl}api/Backload";
                    var token = context.Request.Headers["Authorization"];
                    var postParameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    {"Featured", mediaDto.Featured},
                    {"Title", mediaDto.Title},
                    {"Caption", mediaDto.Caption},
                    {"Description", mediaDto.Description},
                    {"AlternativeText", mediaDto.AlternativeText},
                    {"Keywords", mediaDto.Keywords},
                    {"PublicationDate", mediaDto.PublicationDate},
                    {"MediaType", mediaDto.MediaType},
                    {"Metadata", url}
                };

                    var imageUrl = string.Empty;

                    var data = FormUpload.DownloadData(url);

                    if (data != null)
                    {
                        postParameters.Add("file", new FormUpload.FileParameter(data, mediaDto.Title.Replace(" ", "-") + ".jpg", "image/jpeg"));

                        if (postParameters.Any())
                        {
                            var webResponse = FormUpload.MultipartFormDataPost(postURL, postParameters, token);
                            var fullResponse = string.Empty;

                            using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
                            {
                                fullResponse = responseReader.ReadToEnd();
                                webResponse.Close();
                            }

                            var resultMedia = JsonConvert.DeserializeObject<dynamic>(fullResponse);
                            if (resultMedia != null && resultMedia.Id != null)
                            {
                                var media = new Dtos.MediaDto
                                {
                                    Id = Convert.ToInt32(resultMedia.Id),
                                };
                                return media;
                            }

                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }





        #region Internal Fns
        private async Task<ChunkResult> MergeChunkFile(SessionChunk chunk)
        {
            try
            {
                //File upload directory name
                var uploadDirectoryName = Path.Combine(_appSettings.Media.Sizes.FolderPath, _appSettings.Media.Sizes.OriginalFolderName,
                        ImageStoreHelper.CreateYearMonthDayFolderByPath(Path.Combine(_appSettings.Media.Sizes.FolderPath, _appSettings.Media.Sizes.OriginalFolderName)),
                        chunk.FileName);

                //Naming convention of fragment file
                var partToken = FileSort.PART_NUMBER;

                //Actual name of uploaded file
                var baseFileName = chunk.FileName.Substring(0, chunk.FileName.IndexOf(partToken));

                //According to the naming convention, query all eligible fragment files in the specified directory
                var searchpattern = $"{Path.GetFileName(baseFileName)}{partToken}*";

                //Get the list of all fragment files
                var filesList = Directory.GetFiles(Path.GetDirectoryName(uploadDirectoryName), searchpattern);
                if (!filesList.Any())
                    return new ChunkResult() { Error = true, Message = "No files detected" };

                var mergeFiles = new List<FileSort>();
                foreach (string file in filesList)
                {
                    var sort = new FileSort
                    {
                        FileName = file
                    };

                    baseFileName = file.Substring(0, file.IndexOf(partToken));

                    var fileIndex = file.Substring(file.IndexOf(partToken) + partToken.Length);

                    int.TryParse(fileIndex, out var number);
                    if (number <= 0) { continue; }

                    sort.PartNumber = number;

                    mergeFiles.Add(sort);
                }
                // Sort by slice
                var mergeOrders = mergeFiles.OrderBy(s => s.PartNumber).ToList();

                // Merge files
                using (var fileStream = new FileStream(baseFileName, FileMode.Create))
                {
                    foreach (var fileSort in mergeOrders)
                    {
                        using (FileStream fileChunk = new FileStream(fileSort.FileName, FileMode.Open))
                        {
                            await fileChunk.CopyToAsync(fileStream);
                        }
                    }
                }


                DeleteAssociatedChunkFiles(mergeFiles);
                _uploadSessionManager.ClearUploadSession(chunk.SessionId);

                return new ChunkResult() { Error = false, FilePath = baseFileName };
            }
            catch (Exception ex)
            {
                return new ChunkResult() { Error = true, Message = ex.Message };
            }
        }

        private void DeleteAssociatedChunkFiles(List<FileSort> files)
            => files.ForEach(file => { System.IO.File.Delete(file.FileName); });

        private async Task<Dtos.MediaDto> SaveInDbAndCreateSizes(SessionChunk sessionChunk, IFormCollection formData, string finalFilepath)
        {
            try
            {
                var dto = new Dtos.MediaDto();
                dto.IsEnabled = true;
                dto.SourcePath = GetRelativeMediaPath(finalFilepath);
                dto.FileId = sessionChunk.FileId;
                dto.FileName = Path.GetFileName(finalFilepath);
                dto.FileType = Path.GetExtension(Path.GetFileName(finalFilepath)).ToLower();
                dto.FileSize = FileHelper.GetFileSize(finalFilepath);

                if (ImageStoreHelper.IsImage(finalFilepath))
                {
                    var image = Image.Load(finalFilepath);
                    dto.Width = image.Width;
                    dto.Height = image.Height;
                }

                dto.Title = formData["Title"];
                dto.Caption = formData["Caption"];
                dto.Description = formData["Description"];
                dto.AlternativeText = formData["AlternativeText"];
                dto.Keywords = formData["Keywords"];
                dto.MediaUrl = formData["MediaUrl"];
                dto.MediaType = formData["MediaType"];
                dto.Metadata = formData["Metadata"];

                DateTime result;
                if (DateTime.TryParse(formData["PublicationDate"], out result))
                {
                    dto.PublicationDate = DateTime.Parse(formData["PublicationDate"]);
                }
                else
                {
                    dto.PublicationDate = DateTime.Now;
                }

                if (formData["Featured"].ToString().ToLower() == "true")
                    dto.Featured = true;

                if (!string.IsNullOrWhiteSpace(dto.MediaUrl))
                {
                    if (dto.MediaType == "video")
                        dto.Discriminator = "video";
                    else if (dto.MediaType == "audio")
                        dto.Discriminator = "audio";
                }
                else
                    dto.Discriminator = "image";


                foreach (var key in formData.Keys.Where(x => x.Contains("Gallery")))
                {
                    int galleryId;
                    if (int.TryParse(formData[key], out galleryId))
                    {
                        dto.Galleries.Add(new Dtos.DeleteGalleryDtoBindingModel() { Id = galleryId });
                    }
                }

                foreach (var key in formData.Keys.Where(x => x.Contains("Category")))
                {
                    int categoryId;
                    if (int.TryParse(formData[key], out categoryId))
                    {
                        dto.Categories.Add(new Dtos.DeleteCategoryDtoBindingModel() { Id = categoryId });
                    }
                }

                var extension = Path.GetExtension(finalFilepath).ToLower();

                if (ImageStoreHelper.IsImage(extension) && dto.Discriminator != "audio")
                {

                    dto.Size1Path = ImageStoreHelper.ResizeAndSaveImage(finalFilepath, _appSettings.Media.Sizes.Size1.Width, _appSettings.Media.Sizes.Size1.Height, _appSettings.Media);
                    dto.Size2Path = ImageStoreHelper.ResizeAndSaveImage(finalFilepath, _appSettings.Media.Sizes.Size2.Width, _appSettings.Media.Sizes.Size2.Height, _appSettings.Media);
                    dto.Size3Path = ImageStoreHelper.ResizeAndSaveImage(finalFilepath, _appSettings.Media.Sizes.Size3.Width, _appSettings.Media.Sizes.Size3.Height, _appSettings.Media);
                    dto.Size4Path = ImageStoreHelper.ResizeAndSaveImage(finalFilepath, _appSettings.Media.Sizes.Size4.Width, _appSettings.Media.Sizes.Size4.Height, _appSettings.Media);
                    dto.Size5Path = ImageStoreHelper.ResizeAndSaveImage(finalFilepath, _appSettings.Media.Sizes.Size5.Width, _appSettings.Media.Sizes.Size5.Height, _appSettings.Media);
                    dto.Size6Path = ImageStoreHelper.CreateSizeBodyContentNew(finalFilepath, _appSettings.Media.Sizes.Size6.Width, _appSettings.Media.Sizes.Size6.Height, _appSettings.Media);
                }
                //TODO :Probar éste
                else if (ImageStoreHelper.IsImage(extension) && dto.Discriminator == "audio")
                {
                    dto.Size1Path = _appSettings.Media.SoundCloud.ImageSaveUrl;
                    dto.Size2Path = _appSettings.Media.SoundCloud.ImageSaveUrl;
                    dto.Size3Path = _appSettings.Media.SoundCloud.ImageSaveUrl;
                    dto.Size4Path = _appSettings.Media.SoundCloud.ImageSaveUrl;
                    dto.Size5Path = _appSettings.Media.SoundCloud.ImageSaveUrl;
                }

                var media = await _mediaManager.Add(dto);
                return media;
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return null;
            }
        }

        private string GetRelativeMediaPath(string filePath)
        {
            return filePath
                .Replace(_appSettings.Media.Sizes.FolderPath, string.Empty)
                .Replace("\\", "/")
                .Replace("//", "/");
        }
        #endregion
    }
}
