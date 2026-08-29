using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ray.BackendApi.Attributes;
using Ray.BackendApi.Controllers.ExceptionController;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Utils.Configuration;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/PrintEdition")]
    public class PrintEditionController : BaseApiController
    {
        private readonly IPrintEditionManager _manager;
        private readonly INewsManager _managerNews;
        private readonly AppSettings _appSettings;

        public PrintEditionController(IPrintEditionManager manager, INewsManager managerNews, AppSettings appSettings)
        {
            _manager = manager;
            _managerNews = managerNews;
            _appSettings = appSettings;
        }

        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var PrintEdition = await _manager.GetById(id);

                if (PrintEdition == null)
                    return NotFound();



                return Ok(CMSResponse(PrintEdition));
            });
        }

        public async Task<IActionResult> GetAll(PaginationDto pagination)
        {
            return await TryJsonResultAsync(async () =>
            {
                LoadPagination(pagination, await _manager.Count());
                return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take()), pagination));
            });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(await SolrHelper.ExecuteQuery(SolrCore.PRINTEDITION, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr));
            });
        }

        public async Task<IActionResult> Post(PrintEditionDto printEdition)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                var result = await _manager.Add(printEdition);

                var images = JsonConvert.DeserializeObject<List<ImagesPrintEiditon>>(printEdition.Structure);
                foreach (var image in images)
                {
                    using (var webClient = new WebClient())
                    {
                        byte[] imageBytes = webClient.DownloadData(image.path);


                        //TODO: Sacar configuration manager y ponerlo en appsettings.json
                        var dir = "";
                            //$@"{ConfigurationHelper.GetValue<string>("CMS.Media.FileUpload.Sizes.Folder")}\{ConfigurationHelper.GetValue<string>("CMS.Media.FileUpload.PrintEdition.Folder.Name")}\{DateTime.UtcNow.Year}\{DateTime.UtcNow.Month}\{DateTime.UtcNow.Day}\{result.Id}";

                        Directory.CreateDirectory(dir);

                        string fileName = $"{image.page}.jpg";
                        var filePath = string.Format(@"{0}\{1}", dir, Path.GetFileName(fileName)); //Join path with fileName

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            using (BinaryWriter bw = new BinaryWriter(fs))
                            {
                                bw.Write(imageBytes);
                                bw.Close();
                            }
                            fs.Close();
                        }

                        image.path = $@"/{_appSettings.Media.FileUpload.PrintEditionFolderName}/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{result.Id}/{fileName}";
                    }

                }

                result.Structure = JsonConvert.SerializeObject(images);
                await _manager.Update(result);

                return Ok(CMSResponse(result));
            });
        }

        [HttpPost, Route("ImportNews")]
        public async Task<IActionResult> ImportNews(ImportNewsDtoBindingModel newsDto)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                //TODO: ver codigo comentado
                //if (!string.IsNullOrEmpty(newsDto.ImportImageUrl))
                //{
                //    var media = BackloadController.UploadImageFromUrl(newsDto.ImportImageUrl, "Importada Edición Impresa");
                //    if (media != null && media.Id > 0)
                //    {
                //        newsDto.AssetMedia.Add(new AssetMediaDto { Media = media });
                //        newsDto.Galleries = new List<DeleteGalleryDtoBindingModel>() {
                //                   new DeleteGalleryDtoBindingModel{
                //                       Name = "Galería Importada",
                //                       Media = new List<MediaDto> { media }}
                //                    };
                //    }
                //}

                var result = await _managerNews.Add(newsDto);

                return Ok(CMSResponse(result));
            });
        }


        public async Task<IActionResult> Put(PrintEditionDto printEdition)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Update(printEdition);

                return Ok();
            });
        }

        [Route("ChangePublishState")]
        public async Task<IActionResult> ChangePublishState(int id, bool isPublished)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                var printEdition = await _manager.GetById(id);
                printEdition.IsPublished = isPublished;

                await _manager.Update(printEdition);

                return Ok(CMSResponse(printEdition));
            });
        }

        public async Task<IActionResult> Delete(PrintEditionDto printEdition)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new ModelException(ModelState.GetErrorMessage());

                await _manager.Delete(printEdition);

                return Ok();
            });
        }


    }
}