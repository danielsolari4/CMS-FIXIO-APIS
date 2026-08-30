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
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Processing;
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

                if (!sessionChunk.IsChunkedUpload && MustUploadToCloud)
                    return await SaveFileDirectToCloud(sessionChunk, formData, fileChunk);

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
                        return new FileUploadResult() { Error = true, Message = mergeResult.Message };
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
                {
                    var uploadResult = await _amazonS3Manager.UploadFromFileAsync(fileUploadResult, _appSettings.Media.RemoveAfterUpload);

                    if (uploadResult.Error)
                    {
                        fileUploadResult.Error = true;
                        fileUploadResult.Message = uploadResult.Message;
                    }
                }


                return fileUploadResult;
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return new FileUploadResult() { Error = true, Message = ex.Message };
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
        private async Task<FileUploadResult> SaveFileDirectToCloud(SessionChunk sessionChunk, IFormCollection formData, IFormFile file)
        {
            try
            {
                sessionChunk = _uploadSessionManager.CreateOrUpdateUploadSession(sessionChunk);

                var dto = CreateMediaDto(sessionChunk, formData, sessionChunk.FileName, file.Length);
                var storedPaths = CreateStoredMediaPaths(sessionChunk.FileName);
                dto.SourcePath = storedPaths.OriginalPath;
                var thumbnailPath = string.Empty;

                await using var originalStream = file.OpenReadStream();
                var uploadOriginalResult = await _amazonS3Manager.UploadOneFromStreamAsync(storedPaths.OriginalPath, originalStream, file.ContentType);

                if (uploadOriginalResult.Error)
                    return new FileUploadResult { Error = true, Message = uploadOriginalResult.Message };

                if (ImageStoreHelper.IsImage(file.FileName))
                {
                    await using var imageStream = file.OpenReadStream();
                    using var image = Image.Load(imageStream);

                    dto.Width = image.Width;
                    dto.Height = image.Height;

                    dto.Size1Path = await ResizeAndUploadAsync(image, storedPaths.Size1Path, _appSettings.Media.Sizes.Size1.Width, _appSettings.Media.Sizes.Size1.Height, file.FileName, crop: true);
                    dto.Size2Path = await ResizeAndUploadAsync(image, storedPaths.Size2Path, _appSettings.Media.Sizes.Size2.Width, _appSettings.Media.Sizes.Size2.Height, file.FileName, crop: true);
                    dto.Size3Path = await ResizeAndUploadAsync(image, storedPaths.Size3Path, _appSettings.Media.Sizes.Size3.Width, _appSettings.Media.Sizes.Size3.Height, file.FileName, crop: true);
                    dto.Size4Path = await ResizeAndUploadAsync(image, storedPaths.Size4Path, _appSettings.Media.Sizes.Size4.Width, _appSettings.Media.Sizes.Size4.Height, file.FileName, crop: true);
                    dto.Size5Path = await ResizeAndUploadAsync(image, storedPaths.Size5Path, _appSettings.Media.Sizes.Size5.Width, _appSettings.Media.Sizes.Size5.Height, file.FileName, crop: true);
                    dto.Size6Path = await ResizeAndUploadAsync(image, storedPaths.Size6Path, _appSettings.Media.Sizes.Size6.Width, _appSettings.Media.Sizes.Size6.Height, file.FileName, crop: false);

                    var thumbnailResult = await UploadImageAsync(image, storedPaths.ThumbnailPath, file.FileName, clone =>
                    {
                        clone.Mutate(x => x.Resize(80, 60));
                        return clone;
                    });

                    if (thumbnailResult.Error)
                        return new FileUploadResult { Error = true, Message = thumbnailResult.Message };

                    thumbnailPath = storedPaths.ThumbnailPath;
                }

                var media = await _mediaManager.Add(dto);

                if (media == null)
                    return new FileUploadResult { Error = true, Message = "No se pudo guardar el media en la base de datos." };

                _uploadSessionManager.ClearUploadSession(sessionChunk.SessionId);

                return new FileUploadResult
                {
                    Id = media.Id,
                    Url = storedPaths.OriginalPath,
                    ThumbnailUrl = thumbnailPath,
                    Size1Path = dto.Size1Path,
                    Size2Path = dto.Size2Path,
                    Size3Path = dto.Size3Path,
                    Size4Path = dto.Size4Path,
                    Size5Path = dto.Size5Path,
                    Size6Path = dto.Size6Path,
                    IsChunkingUploads = false,
                    UploadFinished = true,
                    Error = false
                };
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
                return new FileUploadResult { Error = true, Message = ex.Message };
            }
        }

        private MediaDto CreateMediaDto(SessionChunk sessionChunk, IFormCollection formData, string fileName, long fileSize)
        {
            var dto = new MediaDto
            {
                IsEnabled = true,
                FileId = sessionChunk.FileId,
                FileName = Path.GetFileName(fileName),
                FileType = Path.GetExtension(Path.GetFileName(fileName)).ToLower(),
                FileSize = fileSize,
                Title = formData["Title"],
                Caption = formData["Caption"],
                Description = formData["Description"],
                AlternativeText = formData["AlternativeText"],
                Keywords = formData["Keywords"],
                MediaUrl = formData["MediaUrl"],
                MediaType = formData["MediaType"],
                Metadata = formData["Metadata"]
            };

            if (DateTime.TryParse(formData["PublicationDate"], out var publicationDate))
                dto.PublicationDate = publicationDate;
            else
                dto.PublicationDate = DateTime.Now;

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
                if (int.TryParse(formData[key], out var galleryId))
                    dto.Galleries.Add(new DeleteGalleryDtoBindingModel { Id = galleryId });
            }

            foreach (var key in formData.Keys.Where(x => x.Contains("Category")))
            {
                if (int.TryParse(formData[key], out var categoryId))
                    dto.Categories.Add(new DeleteCategoryDtoBindingModel { Id = categoryId });
            }

            return dto;
        }

        private StoredMediaPaths CreateStoredMediaPaths(string fileName)
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;
            var day = DateTime.UtcNow.Day;
            var originalName = Path.GetFileName(fileName);
            var extension = Path.GetExtension(originalName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalName);
            var sizesFolder = NormalizeFolder(_appSettings.Media.Sizes.FolderName);
            var originalFolder = NormalizeFolder(_appSettings.Media.Sizes.OriginalFolderName);
            var thumbsFolder = NormalizeFolder(_appSettings.Media.Sizes.ThumbsFileFolder);

            string InDatedFolder(string folder, string name) => $"/{folder}/{year}/{month}/{day}/{name}".Replace("//", "/");
            string Sized(int width, int height) => $"{nameWithoutExtension}_{width}x{height}{extension}";

            return new StoredMediaPaths
            {
                OriginalPath = InDatedFolder(originalFolder, originalName),
                ThumbnailPath = $"/{thumbsFolder}/{originalName}".Replace("//", "/"),
                Size1Path = InDatedFolder(sizesFolder, Sized(_appSettings.Media.Sizes.Size1.Width, _appSettings.Media.Sizes.Size1.Height)),
                Size2Path = InDatedFolder(sizesFolder, Sized(_appSettings.Media.Sizes.Size2.Width, _appSettings.Media.Sizes.Size2.Height)),
                Size3Path = InDatedFolder(sizesFolder, Sized(_appSettings.Media.Sizes.Size3.Width, _appSettings.Media.Sizes.Size3.Height)),
                Size4Path = InDatedFolder(sizesFolder, Sized(_appSettings.Media.Sizes.Size4.Width, _appSettings.Media.Sizes.Size4.Height)),
                Size5Path = InDatedFolder(sizesFolder, Sized(_appSettings.Media.Sizes.Size5.Width, _appSettings.Media.Sizes.Size5.Height)),
                Size6Path = InDatedFolder(sizesFolder, Sized(_appSettings.Media.Sizes.Size6.Width, _appSettings.Media.Sizes.Size6.Height))
            };
        }

        private async Task<string> ResizeAndUploadAsync(Image image, string key, int width, int height, string fileName, bool crop)
        {
            var uploadResult = await UploadImageAsync(image, key, fileName, clone =>
            {
                if (crop)
                    return ImageStoreHelper.CropFixedSize(clone, width, height);

                return ImageStoreHelper.ResizeImage(clone, width, height);
            });

            if (uploadResult.Error)
                throw new Exception(uploadResult.Message ?? "AmazonS3 upload failed.");

            return key;
        }

        private async Task<AmazonS3FileUploadResultDto> UploadImageAsync(Image source, string key, string fileName, Func<Image, Image> mutate)
        {
            using var outputImage = mutate(source.Clone(x => { }));
            await using var outputStream = new MemoryStream();
            SaveImage(outputImage, outputStream, fileName);
            outputStream.Position = 0;
            return await _amazonS3Manager.UploadOneFromStreamAsync(key, outputStream, GetContentType(fileName));
        }

        private void SaveImage(Image image, Stream stream, string fileName)
        {
            var encoder = GetImageEncoder(fileName);
            image.Save(stream, encoder);
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

        private string GetContentType(string fileName)
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

        private static string NormalizeFolder(string folder)
            => (folder ?? string.Empty).Replace("\\", "/").Trim('/');

        private class StoredMediaPaths
        {
            public string OriginalPath { get; set; }
            public string ThumbnailPath { get; set; }
            public string Size1Path { get; set; }
            public string Size2Path { get; set; }
            public string Size3Path { get; set; }
            public string Size4Path { get; set; }
            public string Size5Path { get; set; }
            public string Size6Path { get; set; }
        }

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
