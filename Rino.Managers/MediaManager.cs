using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers.Core;
using Rino.Managers.MapperProfiles;
using Rino.Model.NewContext.Entities;
using Rino.Repositories;
using Rino.Utils.Configuration;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface IMediaManager : IManager<MediaDto>
    {
        Task<ICollection<MediaDto>> GetAll(int? skip, int? take, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false);
        Task<MediaDto> GetById(int id, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false);
        Task Delete(string fileName);
        Task UpdateCounts(CountDto dto);
        Task<ICollection<MediaDto>> AddMigration(ICollection<MediaDto> dto);
        Task<List<int>> GetUsedMediaIdsAsync(List<int> mediaIds);
    }

    public class MediaManager : BaseManager, IMediaManager
    {
        private readonly IAssetMediaRepository _assetMediaRepository;
        private readonly IMediaRepository _repository;
        private readonly IKeywordRepository _keywordRepository;
        private readonly IGalleryRepository _galleryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly AppSettings _appSettings;
        private readonly IAmazonS3Manager _amazonS3Manager;

        public MediaManager(IMediaRepository repository, IKeywordRepository keywordRepository, IGalleryRepository galleryRepository, ICategoryRepository categoryRepository, IAssetMediaRepository assetMediaRepository, AppSettings appSettings, IAmazonS3Manager amazonS3Manager)
        {
            _repository = repository;
            _keywordRepository = keywordRepository;
            _galleryRepository = galleryRepository;
            _categoryRepository = categoryRepository;
            _assetMediaRepository = assetMediaRepository;
            _appSettings = appSettings;
            _amazonS3Manager = amazonS3Manager;
        }

        public async Task<ICollection<MediaDto>> AddMigration(ICollection<MediaDto> dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var listMedia = new List<Media>();
            foreach (var item in dto)
            {
                listMedia.Add(await MapFromDto(item));
            }

            var newList = await _repository.AddImportAsync(listMedia);

            //var news = (News)await _repository.Add();

            //await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            //await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);
            //await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);

            return dto;
        }
        public async Task<ICollection<MediaDto>> GetAll(int? skip, int? take, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false)
        {
            var media = new List<MediaDto>();

            var mediaSet = await _repository.GetAll();

            if (skip.HasValue)
                mediaSet = mediaSet.OrderBy(m => m.Id).Skip(skip.Value);

            if (take.HasValue)
                mediaSet = mediaSet.Take(take.Value);

            foreach (var m in mediaSet)
                media.Add(MapToDto(m, includeGalleries, includeCategories, includeAssets));

            return media;
        }

        public async Task<List<int>> GetUsedMediaIdsAsync(List<int> mediaIds)
        {
            var media = new List<int>();
            var mediaSet = await _assetMediaRepository.GetUsedMediaIdsAsync(mediaIds);

            foreach (var item in mediaSet)
                media.Add(item);

            return media;
        }

        public async Task<MediaDto> GetById(int id, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var media = await _repository.GetById(id);
            return media != null ? MapToDto(media, includeGalleries, includeCategories, includeAssets) : null;
        }

        public async Task<ICollection<MediaDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(skip, take, false, false, false);
        }

        public async Task<MediaDto> GetById(int id)
        {
            return await GetById(id, false, false, false);
        }

        public async Task<MediaDto> Add(MediaDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException("dto");

                var mediaToAdd = await MapFromDto(dto);


                foreach (var category in mediaToAdd.MediaCategories)
                {
                    if (category.Category != null)
                        category.Category.LastModificationDate = DateTime.UtcNow;
                }
                foreach (var mg in mediaToAdd.MediaGalleries)
                {
                    if (mg.Gallery != null)
                    {
                        mg.Gallery.LastModificationDate = DateTime.UtcNow;
                        mg.Gallery.CacheSolr = false;
                    }
                }

                var result = await _repository.Add(mediaToAdd);



                await SolrHelper.DataImport(SolrCore.MEDIA, _appSettings.Solr);
                await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
                await SolrHelper.DataImport(SolrCore.GALLERY, _appSettings.Solr);

                return MapToDto(result);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task Update(MediaDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("dto");

            var media = await _repository.GetById(dto.Id);

            if (media == null)
                throw new ArgumentNullException("media");

            await _repository.Update(await MapFromDto(dto, media));

            await SolrHelper.DataImport(SolrCore.MEDIA, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.GALLERY, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.CATEGORY, _appSettings.Solr);
        }

        public async Task Delete(MediaDto dto)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public async Task Delete(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("filename");

            var media = (await _repository.Get(m => m.FileName.Equals(fileName))).SingleOrDefault();

            if (media == null)
                throw new ArgumentNullException("media");

            await DeleteMediaFilesFromS3(media);

            // Clears Media-Keyword & Media-Gallery relations
            media.MediaKeywords.Clear();
            media.MediaGalleries.Clear();

            await _repository.Delete(media);
            await SolrHelper.DeleteDocumentById(SolrCore.MEDIA, media.Id, _appSettings.Solr);
        }

        private async Task DeleteMediaFilesFromS3(Media media)
        {
            if (string.IsNullOrWhiteSpace(_appSettings.AmazonS3?.BucketName))
                return;

            var keys = new List<string>();

            if (!string.IsNullOrWhiteSpace(media.SourcePath))
                keys.Add(media.SourcePath);

            if (!string.IsNullOrWhiteSpace(media.SizesPaths))
            {
                try
                {
                    var sizes = JsonConvert.DeserializeObject<Dictionary<string, string>>(media.SizesPaths);
                    if (sizes != null)
                        keys.AddRange(sizes.Values.Where(v => !string.IsNullOrWhiteSpace(v)));
                }
                catch
                {
                    // Keep deleting the original even if SizesPaths is malformed.
                }
            }

            foreach (var key in keys.Distinct())
            {
                try
                {
                    await _amazonS3Manager.DeleteOneAsync(key);
                }
                catch (Exception)
                {
                    // A missing S3 object must not block deleting the database record.
                }
            }
        }

        public async Task UpdateCounts(CountDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("dto");

            var media = await _repository.GetById(dto.Id);

            if (media == null)
                throw new ArgumentNullException("media");

            switch (dto.Discriminator)
            {
                case CountDiscriminator.Share:
                    if (dto.Count != 0)
                    {
                        if (media.ShareCount > 0)
                            media.ShareCount = media.ShareCount + dto.Count;
                        else
                            media.ShareCount = dto.Count;
                    }
                    else
                        throw new ArgumentNullException("count");
                    break;

                case CountDiscriminator.Views:
                    if (dto.Count != 0)
                    {
                        if (media.ViewsCount > 0)
                            media.ViewsCount = media.ViewsCount + dto.Count;
                        else
                            media.ViewsCount = dto.Count;
                    }
                    else
                        throw new ArgumentNullException("count");
                    break;

                default:
                    throw new ArgumentNullException("discriminator");
            }

            await _repository.Update(media);
            await SolrHelper.DataImport(SolrCore.MEDIA, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private MediaDto MapToDto(Media media, bool includeGalleries = false, bool includeCategories = false, bool includeAssets = false)
        {
            InitAddMapper();

            var dto = _Mapper.Map<MediaDto>(media);
            dto.SourcePath = media.SourcePath;

            if (includeGalleries)
            {
                if (media.MediaGalleries != null && media.MediaGalleries.Any())
                {
                    foreach (var m in media.MediaGalleries)
                    {
                        dto.Galleries.Add(_Mapper.Map<DeleteGalleryDtoBindingModel>(m.Gallery));
                    }
                }
            }

            if (includeCategories)
            {
                if (media.MediaCategories != null && media.MediaCategories.Any())
                {
                    foreach (var category in media.MediaCategories)
                    {
                        dto.Categories.Add(_Mapper.Map<DeleteCategoryDtoBindingModel>(category.Category));
                    }
                }
            }

            if (includeAssets)
            {
                if (media.AssetMedia != null && media.AssetMedia.Any())
                {
                    foreach (var asset in media.AssetMedia)
                    {
                        var m = new AssetMediaDto();

                        if (asset.Asset is News)
                            m.Asset = _Mapper.Map<NewsDto>(asset.Asset);
                        else if (asset.Asset is Page)
                            m.Asset = _Mapper.Map<PageDto>(asset.Asset);

                        m.Featured = asset.Featured;
                        dto.AssetMedia.Add(m);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(media.SizesPaths))
            {
                var sizesPaths = JsonConvert.DeserializeObject<dynamic>(media.SizesPaths);
                if (sizesPaths != null)
                {
                    dto.Size1Path = sizesPaths.Size1Path;
                    dto.Size2Path = sizesPaths.Size2Path;
                    dto.Size3Path = sizesPaths.Size3Path;
                    dto.Size4Path = sizesPaths.Size4Path;
                    dto.Size5Path = sizesPaths.Size5Path;
                }
            }

            return dto;
        }

        private async Task<Media> MapFromDto(MediaDto mediaDto, Media media = null)
        {
            Media ret;

            if (media == null)
            {
                InitAddMapper();

                var newMedia = _Mapper.Map<Media>(mediaDto);

                if (!string.IsNullOrWhiteSpace(mediaDto.Keywords))
                {
                    var keywords = mediaDto.Keywords.Split(',').Select(k => k.Trim().ToLower());
                    newMedia.Keywords = string.Empty;

                    foreach (var sKeyword in keywords)
                    {
                        var keyword = (await _keywordRepository.Get(k => k.Name.Equals(sKeyword) && k.IsEnabled)).SingleOrDefault();

                        if (keyword == null)
                            keyword = new Keyword(sKeyword);

                        newMedia.MediaKeywords.Add(new MediaKeyword() { Keyword = keyword });
                        newMedia.Keywords = string.IsNullOrWhiteSpace(newMedia.Keywords) ? sKeyword : string.Format("{0},{1}", newMedia.Keywords, sKeyword);
                    }
                }

                if (mediaDto.Galleries != null)
                {
                    foreach (var galleryDto in mediaDto.Galleries)
                    {
                        var gallery = await _galleryRepository.GetById(galleryDto.Id);

                        if (gallery != null)
                        {
                            var MediaGalleries = new MediaGallery
                            {
                                Gallery = gallery
                            };
                            newMedia.MediaGalleries.Add(MediaGalleries);
                        }
                    }
                }

                if (mediaDto.Categories != null)
                {
                    foreach (var categoryDto in mediaDto.Categories)
                    {
                        var category = await _categoryRepository.GetById(categoryDto.Id);

                        if (category != null)
                        {
                            newMedia.MediaCategories.Add(new MediaCategory() { Category = category });
                        }

                    }
                }

                dynamic sizesPaths = new ExpandoObject();

                sizesPaths.Size1Path = mediaDto.Size1Path;
                sizesPaths.Size2Path = mediaDto.Size2Path;
                sizesPaths.Size3Path = mediaDto.Size3Path;
                sizesPaths.Size4Path = mediaDto.Size4Path;
                sizesPaths.Size5Path = mediaDto.Size5Path;

                newMedia.SizesPaths = JsonConvert.SerializeObject(sizesPaths);
                newMedia.Metadata = mediaDto.Metadata;
                ret = newMedia;
            }
            else
            {
                InitUpdateMapper();

                _Mapper.Map<MediaDto, Media>(mediaDto, media);

                if (!string.IsNullOrEmpty(mediaDto.Size1Path)
                    && !string.IsNullOrEmpty(mediaDto.Size2Path)
                    && !string.IsNullOrEmpty(mediaDto.Size3Path)
                    && !string.IsNullOrEmpty(mediaDto.Size4Path)
                    && !string.IsNullOrEmpty(mediaDto.Size5Path))
                {
                    dynamic sizesPaths = new ExpandoObject();
                    sizesPaths.Size1Path = mediaDto.Size1Path;
                    sizesPaths.Size2Path = mediaDto.Size2Path;
                    sizesPaths.Size3Path = mediaDto.Size3Path;
                    sizesPaths.Size4Path = mediaDto.Size4Path;
                    sizesPaths.Size5Path = mediaDto.Size5Path;
                    media.SizesPaths = JsonConvert.SerializeObject(sizesPaths);
                }

                UpdateKeywords(mediaDto, media);
                await UpdateCategories(mediaDto, media);
                await UpdateGalleries(mediaDto, media);
                media.Metadata = mediaDto.Metadata;
                ret = media;
            }

            ret.CacheSolr = false;

            return ret;
        }

        private async void UpdateKeywords(MediaDto mediaDto, Media media)
        {
            var keywords = !string.IsNullOrWhiteSpace(mediaDto.Keywords) ? mediaDto.Keywords.Split(',').Select(k => k.Trim().ToLower()) : new List<string>();
            media.Keywords = string.Empty;

            foreach (var sKeyword in keywords)
            {
                var keyword = (await _keywordRepository.Get(k => k.Name.Equals(sKeyword) && k.IsEnabled)).SingleOrDefault();

                if (keyword == null)
                    media.MediaKeywords.Add(new MediaKeyword() { Keyword = new Keyword(sKeyword) });
                else
                {
                    if (!media.MediaKeywords.Any(k => k.Keyword.Name.Equals(keyword.Name)))
                        media.MediaKeywords.Add(new MediaKeyword() { Keyword = new Keyword(sKeyword) });
                }

                media.Keywords = string.IsNullOrWhiteSpace(media.Keywords) ? sKeyword : string.Format("{0},{1}", media.Keywords, sKeyword);
            }

            foreach (var keyword in media.MediaKeywords.Where(keyword => !keywords.Any(sKeyword => keyword.Keyword.Name.Equals(sKeyword))).ToArray())
            {
                media.MediaKeywords.Remove(keyword);
            }
        }

        private async Task UpdateCategories(MediaDto mediaDto, Media media)
        {
            if (mediaDto.Categories == null)
                mediaDto.Categories = new List<DeleteCategoryDtoBindingModel>();

            foreach (var categoryDto in mediaDto.Categories.Where(c => (!media.MediaCategories.Any(category => category.CategoryId.Equals(c.Id)))))
            {
                var category = await _categoryRepository.GetById(categoryDto.Id);

                if (category != null && category.IsEnabled)
                {
                    category.LastModificationDate = DateTime.UtcNow;
                    category.CacheSolr = false;
                    media.MediaCategories.Add(new MediaCategory() { CategoryId = category.Id });
                }
            }

            foreach (var category in media.MediaCategories.Where(c => !mediaDto.Categories.Any(categoryDto => c.CategoryId.Equals(categoryDto.Id))).ToArray())
            {
                category.Category.LastModificationDate = DateTime.UtcNow;
                category.Category.CacheSolr = false;
                media.MediaCategories.Remove(category);
            }
        }

        private async Task UpdateGalleries(MediaDto mediaDto, Media media)
        {
            if (mediaDto.Galleries == null)
                mediaDto.Galleries = new List<DeleteGalleryDtoBindingModel>();

            foreach (var galleryDto in mediaDto.Galleries.Where(g => !media.MediaGalleries.Any(gallery => gallery.GalleryId.Equals(g.Id))))
            {
                var gallery = await _galleryRepository.GetById(galleryDto.Id);

                if (gallery != null && gallery.IsEnabled)
                {
                    var MediaGalleries = new MediaGallery
                    {
                        Media = media
                    };
                    gallery.LastModificationDate = DateTime.UtcNow;
                    gallery.CacheSolr = false;
                    gallery.MediaGalleries.Add(MediaGalleries);
                }
            }

            foreach (var gallery in media.MediaGalleries.Where(g => !mediaDto.Galleries.Any(galleryDto => g.GalleryId.Equals(galleryDto.Id))).ToArray())
            {
                gallery.Gallery.LastModificationDate = DateTime.UtcNow;
                gallery.Gallery.CacheSolr = false;
                media.MediaGalleries.Remove(gallery);
            }
        }

        private void InitUpdateMapper()
        {
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<UpdateMediaProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        private void InitAddMapper()
        {
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<CreateMediaProfile>();
                cfg.AddProfile<CategoryProfile>();
                cfg.AddProfile<GalleryProfile>();
                cfg.AddProfile<NewsProfile>();
                cfg.AddProfile<PageProfile>();
                cfg.AddProfile<KeywordProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }
    }
}
