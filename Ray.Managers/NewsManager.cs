using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Core;
using Ray.Managers.MapperProfiles;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Configuration;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface INewsManager : IManager<NewsDto>
    {
        Task<ICollection<NewsDto>> GetAll(bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null, int? skip = null, int? take = null);

        Task<NewsDto> GetById(int id, bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null);

        Task SendApproval(NewsDto dto);

        Task Approve(NewsDto dto);

        Task Disapprove(NewsDto dto);

        Task Publish(NewsDto dto);

        Task PublishArray(int[] ids);

        Task UpdateCounts(CountDto dto);

        Task Alert(NewsDto dto);

        Task<NewsDto> AddFrontEnd(NewsDto dto);

        Task DeleteFrontEnd(NewsDto dto);

        Task UpdateFrontEnd(NewsDto dto);

        Task<ICollection<NewsDto>> AddMigration(ICollection<NewsDto> dto);
        Task Restore(RestoreNewsDtoBindingModel newsDto);

        Task<NewsDto> AddPublish(NewsDto dto);
    }

    public class NewsManager : AssetManager, INewsManager
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly INewsSourceRepository _newsSourceRepository;
        private readonly IPrintEditionNewRepository _printEditionNewRepository;
        private readonly IAssetJsonRepository _assetJsonRepository;
        private readonly AppSettings _appSettings;
        private readonly ICacheInvalidationManager _cacheInvalidation;

        public NewsManager(IAssetRepository repository, IAuthorRepository authorRepository, INodeRepository nodeRepository, IKeywordRepository keywordRepository, IMediaRepository mediaRepository, IGalleryRepository galleryRepository, IUserRepository userRepository, INewsSourceRepository newsSourceRepository, ISocialNetworkRepository socialNetworkRepository, IPrintEditionNewRepository printEditionNewRepository, IAssetJsonRepository assetJsonRepository, AppSettings appSettings, ICacheInvalidationManager cacheInvalidation)
            : base(repository, nodeRepository, galleryRepository, mediaRepository, keywordRepository, userRepository, socialNetworkRepository)
        {
            _authorRepository = authorRepository;
            _newsSourceRepository = newsSourceRepository;
            _printEditionNewRepository = printEditionNewRepository;
            _assetJsonRepository = assetJsonRepository;
            _appSettings = appSettings;
            _cacheInvalidation = cacheInvalidation;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<NewsProfile>();
                cfg.AddProfile<AuthorProfile>();
                cfg.AddProfile<NodeProfile>();
                cfg.AddProfile<KeywordProfile>();
                cfg.AddProfile<UpdateMediaProfile>();
                cfg.AddProfile<GalleryProfile>();
                cfg.AddProfile<NewsSourceProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<NewsDto>> GetAll(bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null, int? skip = null, int? take = null)
        {
            var newsList = new List<NewsDto>();

            var newsSet = (await _repository.Get(n => !n.IsDeleted)).OfType<News>();

            if (skip.HasValue)
                newsSet = newsSet.OrderBy(n => n.Id).Skip(skip.Value);

            if (take.HasValue)
                newsSet = newsSet.Take(take.Value);

            foreach (var news in newsSet)
                newsList.Add(MapToDto(news, includeContent, includeAuthors, includeNodes, includeMedia, includeGalleries, includeRelatedAssets, languageId));

            return newsList;
        }

        public async Task<NewsDto> GetById(int id, bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var news = (await _repository.Get(n => n.Id == id)).OfType<News>().SingleOrDefault();
            return news != null ? MapToDto(news, includeContent, includeAuthors, includeNodes, includeMedia, includeGalleries, includeRelatedAssets, languageId) : null;
        }

        public async Task<ICollection<NewsDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(false, false, false, false, false, false, null, skip, take);
        }

        public async Task<NewsDto> GetById(int id)
        {
            return await GetById(id, false, false, false, false, false, false, null);
        }

        #region Steps

        public async Task<NewsDto> Add(NewsDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var news = await MapFromDto(dto);

            news.Status = AssetStatus.DRAFT;

            foreach (var author in news.NewsAuthors)
            {
                author.Author.LastModificationDate = DateTime.UtcNow;
                author.Author.CacheSolr = false;
            }

            foreach (var relatedAsset in news.AssetAssetRelatedAssets)
            {
                relatedAsset.RelatedAsset.LastModificationDate = DateTime.UtcNow;
                relatedAsset.RelatedAsset.CacheSolr = false;
            }

            news.CacheSolr = false;

            news = (News)await _repository.Add(news);

            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, null, $"news:{news.Id} add");

            return MapToDto(news);
        }

        public async Task<NewsDto> AddPublish(NewsDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var news = await MapFromDto(dto);

            news.Status = AssetStatus.PUBLISHED;

            foreach (var author in news.NewsAuthors)
            {
                author.Author.LastModificationDate = DateTime.UtcNow;
                author.Author.CacheSolr = false;
            }

            foreach (var relatedAsset in news.AssetAssetRelatedAssets)
            {
                relatedAsset.RelatedAsset.LastModificationDate = DateTime.UtcNow;
                relatedAsset.RelatedAsset.CacheSolr = false;
            }

            news.CacheSolr = false;

            news = (News)await _repository.Add(news);

            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, null, $"news:{news.Id} add-publish");

            return MapToDto(news);
        }

        public async Task<ICollection<NewsDto>> AddMigration(ICollection<NewsDto> dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var listNews = new List<News>();
            foreach (var item in dto)
            {
                listNews.Add(await MapMigrationAsync(item));
            }

            //var newList = await _repository.AddImportAsync(listNews);

            //var news = (News)await _repository.Add();

            //await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            //await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);
            //await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);

            return dto;
        }

        #region Front End CRUD

        public async Task<NewsDto> AddFrontEnd(NewsDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("dto");

            var news = await MapFromDto(dto);
            news.Status = AssetStatus.DRAFT;
            news.IsEnabled = true;
            news.NewsSource = (await _newsSourceRepository.Get(n => n.Name.ToLower().Equals("mi reporte"))).FirstOrDefault();

            news = (News)await _repository.Add(news);

            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            return MapToDto(news);
        }

        public async Task UpdateFrontEnd(NewsDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("dto");

            var news = (await _repository.Get(n => n.Id == dto.Id && !n.IsDeleted && n.Status.Equals(AssetStatus.DRAFT))).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new ArgumentNullException("news");

            await _repository.Update(await MapFromDto(dto, news));
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
        }

        public async Task DeleteFrontEnd(NewsDto dto)
        {
            var news = (await _repository.Get(n => n.Id == dto.Id && !n.IsDeleted && n.Status.Equals(AssetStatus.DRAFT))).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new ArgumentNullException("news");

            news.IsEnabled = false;
            news.IsDeleted = true;

            news.AssetAssetRelatedAssets.Clear();
            news.AssetAssetAssets.Clear();

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, null, $"news:{news.Id} send-approval");
        }

        #endregion
        public async Task SendApproval(NewsDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var news = (await _repository.Get(n => n.Id == dto.Id)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new EntityException("news");

            if (string.IsNullOrWhiteSpace(dto.LastModificationUser))
            {
                var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == dto.LastModificationUser);

                if (user != null && !user.UserRoles.Any(h => h.Role.Name.Contains("Admin")))
                {
                    if (!news.Status.Equals(AssetStatus.DRAFT))
                        throw new Exception("NSEX_001");
                }
            }

            news.Status = AssetStatus.PENDING_APPROVAL;
            news.CacheSolr = false;

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, null, $"news:{news.Id} approve");
        }

        public async Task Approve(NewsDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var news = (await _repository.Get(n => n.Id == dto.Id)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new EntityException("news");

            if (string.IsNullOrWhiteSpace(dto.LastModificationUser))
            {
                var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == dto.LastModificationUser);

                if (user != null && !user.UserRoles.Any(h => h.Role.Name.Contains("Admin")))
                {
                    if (!news.Status.Equals(AssetStatus.PENDING_APPROVAL))
                        throw new Exception("NSEX_002");
                }
            }

            news.Status = AssetStatus.APPROVED;
            news.CacheSolr = false;

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
        }

        public async Task Disapprove(NewsDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("dto");

            var news = (await _repository.Get(n => n.Id == dto.Id)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new ArgumentNullException("news");

            if (string.IsNullOrWhiteSpace(dto.LastModificationUser))
            {
                var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == dto.LastModificationUser);

                if (user != null && !user.UserRoles.Any(h => h.Role.Name.Contains("Admin")))
                {
                    if (!news.Status.Equals(AssetStatus.PENDING_APPROVAL))
                        throw new Exception("NSEX_003");
                }
            }

            var previous = await _cacheInvalidation.CaptureNewsSnapshot(dto.Id);

            news.Status = AssetStatus.DRAFT;
            news.CacheSolr = false;

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, previous, $"news:{news.Id} disapprove");
        }

        public async Task Publish(NewsDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var news = (await _repository.Get(n => n.Id == dto.Id)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new EntityException("news");

            if (string.IsNullOrWhiteSpace(dto.LastModificationUser))
            {
                var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == dto.LastModificationUser);

                if (user != null && !user.UserRoles.Any(h => h.Role.Name.Contains("Admin")))
                {
                    if (!news.Status.Equals(AssetStatus.APPROVED))
                        throw new Exception("NSEX_004");
                }
            }

            news.Status = AssetStatus.PUBLISHED;
            //news.PublicationDate = DateTime.UtcNow;
            news.WasPublished = true;

            news.CacheSolr = false;

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, null, $"news:{news.Id} publish");
        }

        public async Task PublishArray(int[] newsIds)
        {
            var news = await _repository.GetAll();
            var all = news.Where(x => newsIds.Contains(x.Id)).OfType<News>().ToList();

            if (all == null || !all.Any())
                throw new EntityException("news");

            foreach (var it in all)
            {
                it.Status = AssetStatus.PUBLISHED;
                it.PublicationDate = DateTime.UtcNow;
                it.WasPublished = true;

                it.CacheSolr = false;

                await _repository.Update(it);
                await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            }

            _cacheInvalidation.InvalidateNews(all.Select(x => x.Id).ToList(), $"news publish-array:{all.Count}");

        }

        #endregion

        public async Task Update(NewsDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var news = (await _repository.Get(n => n.Id == dto.Id && !n.IsDeleted)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new EntityException("news");


            var previous = await _cacheInvalidation.CaptureNewsSnapshot(dto.Id);

            await _repository.Update(await MapFromDto(dto, news));
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(dto.Id, previous, $"news:{dto.Id} update");
        }

        public async Task Delete(NewsDto dto)
        {
            var news = (await _repository.Get(n => n.Id == dto.Id && !n.IsDeleted)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new EntityException("news");

            var previous = await _cacheInvalidation.CaptureNewsSnapshot(dto.Id);

            news.IsEnabled = false;
            news.IsDeleted = true;
            news.CacheSolr = false;

            news.AssetAssetAssets.Clear();
            news.AssetAssetRelatedAssets.Clear();

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, previous, $"news:{news.Id} delete");
        }

        public async Task UpdateCounts(CountDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var news = (await _repository.Get(p => p.Id == dto.Id && !p.IsDeleted)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new EntityException("news");

            UpdateCount(dto, news);

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }
        public async Task Alert(NewsDto dto)
        {
            var news = (await _repository.Get(n => n.Id == dto.Id && !n.IsDeleted && (n.Status.Equals(AssetStatus.APPROVED) || n.Status.Equals(AssetStatus.PUBLISHED)))).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new ArgumentNullException("news");

            //var alertedNews = (await _repository.GetAll()).OfType<News>().Where(n => n.IsAlert && n.Id != news.Id).ToList();
            //foreach (var alerted in alertedNews)
            //{
            //    alerted.IsAlert = false;
            //    await _repository.Update(alerted);
            //}

            var previous = await _cacheInvalidation.CaptureNewsSnapshot(dto.Id);

            if (!news.IsAlert)
                news.IsAlert = true;
            else
                news.IsAlert = false;

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, previous, $"news:{news.Id} alert");
        }

        private NewsDto MapToDto(News news, bool includeContent = false, bool includeAuthors = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, bool includeRelatedAssets = false, int? languageId = null)
        {
            var dto = _Mapper.Map<NewsDto>(news);

            if (includeContent)
            {
                foreach (var content in news.AssetContents.OfType<PageContent>().Where(c => !languageId.HasValue || c.LanguageId == languageId.Value))
                {
                    dto.Content.Add(_Mapper.Map<PageContentDto>(content));

                    foreach (var sn in content.PageContentSocialNetworks)
                    {
                        var socialNetworkDto = new SocialNetworkDto
                        {
                            Id = sn.SocialNetworkId,
                            Url = sn.Url,
                            Name = sn.SocialNetwork.Name
                        };
                        foreach (var snDto in dto.Content)
                        {
                            snDto.SocialNetwork.Add(socialNetworkDto);
                        }
                    }
                }
            }

            if (news.PrintEditionNews != null && news.PrintEditionNews.Any())
            {
                dto.PrintEditionNew = new List<PrintEditionNewDto>();
                foreach (var it in news.PrintEditionNews)
                {
                    dto.PrintEditionNew.Add(new PrintEditionNewDto
                    {
                        Id = it.Id,
                        Page = it.Page,
                        PrintEditionId = it.PrintEditionId,
                        PrintEdition = new PrintEditionDto()
                        {
                            NodeId = it?.PrintEdition?.NodeId ?? 0
                        }
                    });
                }
            }

            if (news.AssetJsons != null && news.AssetJsons.Any())
            {
                dto.AssetJson = new List<AssetJsonDto>();
                foreach (var it in news.AssetJsons)
                {
                    var itemParse = JsonConvert.DeserializeObject<AssetJsonDto>(it.Json);
                    itemParse.Id = it.Id;
                    itemParse.AssetId = it.AssetId;
                    dto.AssetJson.Add(itemParse);
                }
            }

            if (includeAuthors)
            {
                foreach (var author in news.NewsAuthors)
                {
                    dto.Authors.Add(_Mapper.Map<AuthorDto>(author.Author));
                }
            }

            if (includeNodes)
            {
                foreach (var node in news.AssetNodes)
                {
                    dto.Nodes.Add(_Mapper.Map<DeleteNodeDtoBindingModel>(node.Node));
                }
            }

            if (includeMedia)
            {
                foreach (var assetMedia in news.AssetMedia)
                {
                    var assetMediaDto = new AssetMediaDto
                    {
                        Featured = assetMedia.Featured,
                        Media = _Mapper.Map<MediaDto>(assetMedia.Media)
                    };

                    if (!string.IsNullOrWhiteSpace(assetMedia.Media.SizesPaths))
                    {
                        var sizesPaths = JsonConvert.DeserializeObject<dynamic>(assetMedia.Media.SizesPaths);
                        if (sizesPaths != null)
                        {
                            assetMediaDto.Media.Size1Path = _appSettings.Content.AdminDomain + sizesPaths.Size1Path;
                            assetMediaDto.Media.Size2Path = _appSettings.Content.AdminDomain + sizesPaths.Size2Path;
                            assetMediaDto.Media.Size3Path = _appSettings.Content.AdminDomain + sizesPaths.Size3Path;
                            assetMediaDto.Media.Size4Path = _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                            assetMediaDto.Media.Size5Path = _appSettings.Content.AdminDomain + sizesPaths.Size5Path;
                        }
                    }

                    dto.AssetMedia.Add(assetMediaDto);
                }
            }

            if (includeGalleries)
            {
                foreach (var gallery in news.Galleries)
                {
                    var galleryDto = _Mapper.Map<DeleteGalleryDtoBindingModel>(gallery.Gallery);

                    if (includeMedia)
                    {
                        foreach (var media in gallery.Gallery.MediaGalleries)
                        {
                            var mediaDto = _Mapper.Map<MediaDto>(media.Media);

                            if (!string.IsNullOrWhiteSpace(media.Media.SizesPaths))
                            {
                                var sizesPaths = JsonConvert.DeserializeObject<dynamic>(media.Media.SizesPaths);
                                if (sizesPaths != null)
                                {
                                    mediaDto.Size1Path = _appSettings.Content.AdminDomain + sizesPaths.Size1Path;
                                    mediaDto.Size2Path = _appSettings.Content.AdminDomain + sizesPaths.Size2Path;
                                    mediaDto.Size3Path = _appSettings.Content.AdminDomain + sizesPaths.Size3Path;
                                    mediaDto.Size4Path = _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                                    mediaDto.Size5Path = _appSettings.Content.AdminDomain + sizesPaths.Size5Path;
                                }
                            }

                            galleryDto.Media.Add(mediaDto);
                        }
                    }

                    dto.Galleries.Add(galleryDto);
                }
            }

            if (includeRelatedAssets)
            {
                foreach (var relatedAsset in news.AssetAssetAssets.Union(news.AssetAssetRelatedAssets))
                {
                    var relatedAssetDto = _Mapper.Map<NewsDto>(relatedAsset.Asset);

                    relatedAssetDto.Content = new List<PageContentDto>();
                    var content = relatedAsset.Asset.AssetContents.OfType<PageContent>().FirstOrDefault();

                    if (content != null)
                    {
                        relatedAssetDto.Content.Add(_Mapper.Map<PageContentDto>(content));
                    }

                    relatedAssetDto.Nodes = new List<DeleteNodeDtoBindingModel>();
                    foreach (var node in relatedAsset.Asset.AssetNodes)
                    {
                        var nodeDto = _Mapper.Map<DeleteNodeDtoBindingModel>(node.Node);
                        nodeDto.Content = new List<NodeContentDto>();
                        var nodeContent = node?.Node?.Content?.OfType<NodeContent>().FirstOrDefault();
                        if (nodeContent != null)
                        {
                            nodeDto.Content.Add(_Mapper.Map<NodeContentDto>(nodeContent));
                        }

                        relatedAssetDto.Nodes.Add(nodeDto);
                    }

                    dto.RelatedAssets.Add(relatedAssetDto);
                }
            }

            return dto;
        }

        private async Task<News> MapFromDto(NewsDto newsDto, News news = null)
        {
            News ret;

            if (news == null)
            {
                var newNews = _Mapper.Map<News>(newsDto);

                if (newsDto.Authors != null && newsDto.Authors.Any())
                {
                    foreach (var authorDto in newsDto.Authors)
                    {
                        var author = await _authorRepository.GetById(authorDto.Id);

                        if (author != null)
                            newNews.NewsAuthors.Add(new NewsAuthor() { AuthorId = author.Id, Author = author });
                    }
                }

                if (newsDto.Nodes != null && newsDto.Nodes.Any())
                {
                    foreach (var nodeDto in newsDto.Nodes)
                    {
                        var node = await _nodeRepository.GetById(nodeDto.Id);

                        if (node != null)
                            newNews.AssetNodes.Add(new AssetNode { NodeId = node.Id });
                    }
                }

                if (newsDto.AssetMedia != null && newsDto.AssetMedia.Any())
                {
                    foreach (var assetMediaDto in newsDto.AssetMedia)
                    {
                        var media = await _mediaRepository.GetById(assetMediaDto.Media.Id);

                        if (media == null)
                            throw new EntityException("media");

                        var aseetMedia = new AssetMedia
                        {
                            Media = media,
                            Featured = assetMediaDto.Featured
                        };
                        newNews.AssetMedia.Add(aseetMedia);
                    }
                }

                if (newsDto.Galleries != null && newsDto.Galleries.Any())
                {
                    foreach (var galleryDto in newsDto.Galleries)
                    {
                        var gallery = await _galleryRepository.GetById(galleryDto.Id);

                        if (gallery == null && galleryDto.Id != 0)
                            throw new EntityException("gallery");

                        if (gallery == null && galleryDto.Id == 0)
                        {
                            //gallery = new Gallery();
                            //if (galleryDto.Media != null && galleryDto.Media.Any())
                            //{
                            //    if (gallery.MediaGalleries == null)
                            //        gallery.MediaGalleries = new List<MediaGallery>();
                            //    foreach (var item in galleryDto.Media)
                            //    {
                            //        var media = await _mediaRepository.GetById(item.Id);

                            //        gallery.MediaGalleries.Add(new MediaGallery
                            //        {
                            //            Media = media,
                            //            MediaId = item.Id
                            //        });
                            //        gallery.CacheSolr = false;
                            //        var galleryInsert = await _galleryRepository.Add(gallery);
                            //    }

                            //}


                            var newGallery = _Mapper.Map<Gallery>(galleryDto);

                            if (galleryDto.Media != null && galleryDto.Media.Any())
                            {
                                foreach (var m in galleryDto.Media)
                                {
                                    var media = await _mediaRepository.GetById(m.Id);

                                    if (media == null)
                                        throw new ArgumentNullException("media");

                                    var mediaGallery = new MediaGallery
                                    {
                                        Media = media,
                                        Order = m.Order
                                    };
                                    newGallery.MediaGalleries.Add(mediaGallery);
                                }
                            }
                            var galleryInsert = await _galleryRepository.Add(newGallery);
                            newNews.Galleries.Add(new AssetGallery() { GalleryId = galleryInsert.Id });
                        }
                        else
                        {

                            if (string.IsNullOrEmpty(gallery.Name?.Trim()))
                                gallery.Name = "Galería";

                            newNews.Galleries.Add(new AssetGallery() { GalleryId = gallery.Id, Gallery = gallery });
                        }
                    }
                }

                if (newsDto.NewsSource != null)
                {
                    var newsSource = await _newsSourceRepository.GetById(newsDto.NewsSource.Id);

                    if (newsSource == null)
                    {
                        var defaultNewsSource = (await _newsSourceRepository.Get(h => h.Name.ToLower().Equals("default"))).FirstOrDefault();
                        news.NewsSourceId = defaultNewsSource.Id;
                    }
                    else
                        news.NewsSourceId = newsSource.Id;
                }

                if (newsDto.RelatedAssets != null && newsDto.RelatedAssets.Any())
                {
                    newNews.AssetAssetRelatedAssets = new List<AssetAsset>();
                    foreach (var relatedAssetDto in newsDto.RelatedAssets)
                    {
                        //var relatedAsset = await _repository.GetById(relatedAssetDto.Id);

                        //if (relatedAsset == null || relatedAsset.IsDeleted)
                        //    throw new EntityException("related_asset");

                        newNews.AssetAssetRelatedAssets.Add(new AssetAsset() { AssetId = relatedAssetDto.Id });
                    }
                }

                if (newsDto.PrintEditionNew != null && newsDto.PrintEditionNew.Any())
                {
                    newNews.PrintEditionNews = new List<PrintEditionNew>();
                    foreach (var it in newsDto.PrintEditionNew)
                    {
                        newNews.PrintEditionNews.Add(new PrintEditionNew
                        {
                            PrintEditionId = it.PrintEditionId,
                            NewId = it.NewId,
                            Page = it.Page
                        });
                    }

                    //news.Status.Equals(AssetStatus.APPROVED)
                }

                if (newsDto.AssetJson != null && newsDto.AssetJson.Any() && newNews != null)
                {
                    foreach (var item in newsDto.AssetJson)
                    {
                        var prop = (AssetJsonProperties)item;
                        var json = JsonConvert.SerializeObject(prop);
                        newNews.AssetJsons.Add(new AssetJson { Json = json });
                    }
                }

                newNews.AccessTypeId = newsDto.AccessTypeId;
                ret = newNews;
            }
            else
            {
                _Mapper.Map<NewsDto, News>(newsDto, news);

                if (newsDto.PrintEditionNew != null && !news.PrintEditionNews.Any())
                {
                    news.PrintEditionNews = new List<PrintEditionNew>();
                    foreach (var it in newsDto.PrintEditionNew)
                    {
                        news.PrintEditionNews.Add(new PrintEditionNew
                        {
                            PrintEditionId = it.PrintEditionId,
                            NewId = it.NewId,
                            Page = it.Page
                        });
                    }
                }
                else if (newsDto.PrintEditionNew != null && news.PrintEditionNews.Any())
                {
                    var elementsToRemove = news.PrintEditionNews.Where(p => newsDto.PrintEditionNew.All(p2 => p2.PrintEditionId != p.PrintEditionId));
                    var elementsToAdd = newsDto.PrintEditionNew.Where(p => news.PrintEditionNews.All(p2 => p2.PrintEditionId != p.PrintEditionId));

                    foreach (var it in elementsToAdd)
                    {
                        news.PrintEditionNews.Add(new PrintEditionNew
                        {
                            PrintEditionId = it.PrintEditionId,
                            NewId = it.NewId,
                            Page = it.Page
                        });
                    }

                    var itemsToBeRemoved = elementsToRemove.Select(it => news.PrintEditionNews.FirstOrDefault(x => x.PrintEditionId == it.PrintEditionId)).ToList();
                    foreach (var it in itemsToBeRemoved)
                    {
                        var PrintEditionNew = await _printEditionNewRepository.GetById(it.Id);
                        if (PrintEditionNew != null)
                            await _printEditionNewRepository.Delete(PrintEditionNew);
                    }


                    foreach (var pe in news.PrintEditionNews)
                    {
                        var peDto = newsDto.PrintEditionNew.FirstOrDefault(p2 => p2.PrintEditionId == pe.PrintEditionId);
                        if (peDto != null)
                            pe.Page = peDto.Page;
                    }

                }
                else if (newsDto.PrintEditionNew == null)
                    if (news.PrintEditionNews != null && news.PrintEditionNews.Any())
                    {
                        var listToRemove = news.PrintEditionNews.ToList();
                        foreach (var it in listToRemove)
                        {
                            var PrintEditionNew = await _printEditionNewRepository.GetById(it.Id);
                            if (PrintEditionNew != null)
                                await _printEditionNewRepository.Delete(PrintEditionNew);
                        }

                    }


                if (newsDto.AssetJson != null && (news.AssetJsons == null || !news.AssetJsons.Any()))
                {
                    news.AssetJsons = new List<AssetJson>();
                    foreach (var it in newsDto.AssetJson)
                    {
                        AssetJsonProperties prop = it;
                        var json = JsonConvert.SerializeObject(prop);
                        news.AssetJsons.Add(new AssetJson
                        {
                            Json = json,
                            AssetId = it.AssetId,
                            Id = it.Id
                        });
                    }
                }
                else if (newsDto.AssetJson != null && news.AssetJsons.Any())
                {
                    var elementsToRemove = news.AssetJsons.Where(p => newsDto.AssetJson.All(p2 => p2.Id != p.Id));
                    var elementsToAdd = newsDto.AssetJson.Where(p => news.AssetJsons.All(p2 => p2.Id != p.Id));

                    foreach (var it in elementsToAdd)
                    {
                        var prop = (AssetJsonProperties)it;
                        var json = JsonConvert.SerializeObject(prop);
                        news.AssetJsons.Add(new AssetJson
                        {
                            Json = json,
                            AssetId = it.AssetId,
                            Id = it.Id
                        });
                    }

                    var itemsToBeRemoved = elementsToRemove.Select(it => news.AssetJsons.FirstOrDefault(x => x.Id == it.Id)).ToList();
                    foreach (var it in itemsToBeRemoved)
                    {
                        var AssetJson = await _assetJsonRepository.GetById(it.Id);
                        if (AssetJson != null)
                            await _assetJsonRepository.Delete(AssetJson);
                    }


                    foreach (var pe in news.AssetJsons)
                    {
                        var peDto = newsDto.AssetJson.FirstOrDefault(p2 => p2.Id == pe.Id);
                        var prop = (AssetJsonProperties)peDto;
                        var json = JsonConvert.SerializeObject(prop);
                        if (peDto != null)
                            pe.Json = json;
                    }

                }
                else if (newsDto.AssetJson == null)
                    if (news.AssetJsons != null && news.AssetJsons.Any())
                    {
                        var listToRemove = news.AssetJsons.ToList();
                        foreach (var it in listToRemove)
                        {
                            var assetJson = await _assetJsonRepository.GetById(it.Id);
                            if (assetJson != null)
                                await _assetJsonRepository.Delete(assetJson);
                        }

                    }

                if (newsDto.NewsSource != null && newsDto.NewsSource.Id != news.NewsSourceId)
                {
                    var newsSource = await _newsSourceRepository.GetById(newsDto.NewsSource.Id);

                    if (newsSource == null)
                    {
                        var defaultNewsSource = (await _newsSourceRepository.Get(h => h.Name.ToLower().Equals("default"))).FirstOrDefault();
                        news.NewsSourceId = defaultNewsSource.Id;
                    }
                    else
                        news.NewsSourceId = newsSource.Id;
                }

                if (newsDto.IsAlert != news.IsAlert)
                {
                    if (news.Status.Equals(AssetStatus.APPROVED) || news.Status.Equals(AssetStatus.PUBLISHED))
                        await Alert(newsDto);
                    else
                        throw new Exception("NSEX_005");
                }

                await UpdateAuthors(newsDto, news);
                await UpdateNodes(newsDto, news);
                await UpdateMedia(newsDto, news);
                await UpdateGalleries(newsDto, news);
                await UpdateRelatedAssets(newsDto, news);

                news.AccessTypeId = newsDto.AccessTypeId;
                ret = news;
            }


            if (newsDto.Content != null)
            {
                foreach (var content in newsDto.Content)
                {

                    var existingContent = ret.AssetContents.OfType<PageContent>().SingleOrDefault(c => c.LanguageId == content.LanguageId);

                    if (existingContent != null)
                    {
                        _Mapper.Map<PageContentDto, PageContent>(content, existingContent);

                        if (string.IsNullOrWhiteSpace(existingContent.MobileTitle))
                            existingContent.MobileTitle = existingContent.Title;

                        UpdateKeywords(newsDto, existingContent);
                        await UpdateSocialNetworks(newsDto, existingContent);
                    }
                    else
                    {
                        var newContent = _Mapper.Map<PageContent>(content);
                        newContent.LanguageId = content.LanguageId;


                        if (string.IsNullOrWhiteSpace(newContent.MobileTitle))
                            newContent.MobileTitle = newContent.Title;

                        if (!string.IsNullOrWhiteSpace(content.Keywords))
                        {
                            var keywords = content.Keywords.Split(',').Select(k => k.Trim().ToLower());
                            newContent.Keywords = string.Empty;

                            foreach (var sKeyword in keywords)
                            {
                                var keyword = (await _keywordRepository.Get(k => k.Name.Equals(sKeyword) && k.IsEnabled)).FirstOrDefault();

                                if (keyword == null)
                                    keyword = new Keyword(sKeyword);

                                newContent.PageContentKeywords.Add(new PageContentKeyword() { Keyword = keyword });
                                newContent.Keywords = string.IsNullOrWhiteSpace(newContent.Keywords) ? sKeyword : string.Format("{0},{1}", newContent.Keywords, sKeyword);
                            }
                        }

                        if (content.SocialNetwork.Any() && content.SocialNetwork != null)
                        {
                            foreach (var snDto in content.SocialNetwork)
                            {
                                var socialNetwork = await _socialNetworkRepository.GetById(snDto.Id);

                                if (socialNetwork == null || !socialNetwork.IsEnabled)
                                    throw new EntityException("social_network");

                                var pageContentSocialNetwork = new PageContentSocialNetwork
                                {
                                    SocialNetwork = socialNetwork,
                                    Url = snDto.Url
                                };
                                newContent.PageContentSocialNetworks.Add(pageContentSocialNetwork);
                            }
                        }

                        ret.AssetContents.Add(newContent);
                    }
                }
            }


            ret.CacheSolr = false;
            ret.Discriminator = "News";
            return ret;
        }

        private async Task UpdateAuthors(NewsDto newsDto, News news)
        {
            var authorsToRemove = news.NewsAuthors.Where(author => !newsDto.Authors.Any(authorDto => authorDto.Id == author.AuthorId)).ToArray();

            if (authorsToRemove != null && authorsToRemove.Any())
            {
                foreach (var it in authorsToRemove)
                {
                    it.Author.LastModificationDate = DateTime.UtcNow;
                    it.Author.CacheSolr = false;
                    news.NewsAuthors.Remove(it);
                }
            }

            var authorsToAdd = newsDto.Authors.Where(authorDto => !news.NewsAuthors.Any(author => author.AuthorId == authorDto.Id));

            if (authorsToAdd != null && authorsToAdd.Any())
                foreach (var authorDto in authorsToAdd)
                {
                    var author = await _authorRepository.GetById(authorDto.Id);

                    if (author == null)
                        throw new EntityException("author");

                    author.LastModificationDate = DateTime.UtcNow;
                    author.CacheSolr = false;
                    news.NewsAuthors.Add(new NewsAuthor() { AuthorId = author.Id });
                }
        }

        private async Task UpdateRelatedAssets(NewsDto newsDto, News news)
        {
            if (newsDto.RelatedAssets.Any() && newsDto.RelatedAssets != null)
            {
                var relatedAssetsToRemove = news.AssetAssetRelatedAssets.Where(relatedAsset => !newsDto.RelatedAssets.Any(relatedAssetDto => relatedAssetDto.Id == relatedAsset.AssetId)).ToArray();

                if (relatedAssetsToRemove != null && relatedAssetsToRemove.Any())
                    foreach (var it in relatedAssetsToRemove)
                    {
                        it.RelatedAsset.LastModificationDate = DateTime.UtcNow;
                        it.RelatedAsset.CacheSolr = false;
                        news.AssetAssetRelatedAssets.Remove(it);
                    }

                var relatedAssetsToAdd = newsDto.RelatedAssets.Where(relatedAssetDto => !news.AssetAssetRelatedAssets.Any(relatedAsset => relatedAsset.AssetId == relatedAssetDto.Id) && !news.AssetAssetAssets.Any(relatedAsset => relatedAsset.AssetId == relatedAssetDto.Id));

                if (relatedAssetsToAdd != null && relatedAssetsToAdd.Any())
                    foreach (var relatedAssetDto in relatedAssetsToAdd)
                    {
                        var relatedAsset = await _repository.GetById(relatedAssetDto.Id);

                        if (relatedAsset == null || relatedAsset.IsDeleted)
                            throw new EntityException("related_asset");

                        relatedAsset.LastModificationDate = DateTime.UtcNow;
                        relatedAsset.CacheSolr = false;
                        news.AssetAssetRelatedAssets.Add(new AssetAsset() { AssetId = relatedAsset.Id });
                    }
            }
        }

        private async Task<News> MapMigrationAsync(NewsDto dto)
        {
            var news = new News
            {
                NewsAuthors = new List<NewsAuthor> { },
                AssetContents = new List<AssetContent> {
                    new PageContent(){
                        Title = dto.Content[0].Title,
                        SocialNetworkTitle= dto.Content[0].SocialNetworkTitle,
                        Content = dto.Content[0].Content,
                        Description = dto.Content[0].Description,
                        MobileTitle = dto.Content[0].MobileTitle,
                        LanguageId = 1,
                        TemplateId = 1,
                        CreationUser = "System",
                        CreationDate = DateTime.Now,
                        Volanta = dto.Content[0].Volanta,
                        PageContentKeywords = new List<PageContentKeyword>()
                    }
                },
                AssetNodes = new List<AssetNode> { }
            };

            news.PublicationDate = dto.PublicationDate;
            news.CreationDate = dto.CreationDate;
            news.LastModificationDate = dto.LastModificationDate;
            news.CreationUser = dto.CreationUser;
            news.LastModificationUser = dto.LastModificationUser;
            news.Url = dto.Url;
            news.DataExtension = dto.DataExtension;
            news.Status = dto.Status;
            news.AccessTypeId = dto.AccessTypeId;



            if (!string.IsNullOrWhiteSpace(dto.Content[0].Keywords))
            {
                var keywords = dto.Content[0].Keywords.Split(',').Select(k => k.Trim().ToLower());
                var firstContent = (PageContent)news.AssetContents.First();

                foreach (var sKeyword in keywords)
                {
                    var keyword = (await _keywordRepository.Get(k => k.Name.Equals(sKeyword) && k.IsEnabled)).FirstOrDefault();

                    if (keyword == null)
                        keyword = new Keyword(sKeyword);


                    firstContent.PageContentKeywords.Add(new PageContentKeyword() { Keyword = keyword });
                    firstContent.Keywords = string.IsNullOrWhiteSpace(firstContent.Keywords) ? sKeyword : string.Format("{0},{1}", firstContent.Keywords, sKeyword);
                }
            }

            if (dto.Authors.Any())
            {
                var authord = await _authorRepository.GetById(dto.Authors[0].Id);
                if (authord != null)
                    news.NewsAuthors.Add(new NewsAuthor { AuthorId = authord.Id });
            }

            if (dto.Nodes.Any())
            {
                var node = await _nodeRepository.GetById(dto.Nodes[0].Id);
                if (node != null)
                    news.AssetNodes.Add(new AssetNode() { NodeId = node.Id });
            }

            foreach (var it in news.NewsAuthors)
            {
                it.Author.LastModificationDate = DateTime.UtcNow;
                it.Author.CacheSolr = false;
            }

            foreach (var it in news.AssetAssetRelatedAssets)
            {
                it.RelatedAsset.LastModificationDate = DateTime.UtcNow;
                it.RelatedAsset.CacheSolr = false;
            }


            return news;
        }

        public async Task Restore(RestoreNewsDtoBindingModel newsDto)
        {
            var news = (await _repository.Get(n => n.Id == newsDto.Id && n.IsDeleted)).OfType<News>().SingleOrDefault();

            if (news == null)
                throw new EntityException("news");

            news.IsDeleted = false;
            news.CacheSolr = false;

            await _repository.Update(news);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            _cacheInvalidation.InvalidateNews(news.Id, null, $"news:{news.Id} restore");
        }

    }
}
