using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    public interface IPageManager : IManager<PageDto>
    {
        Task<ICollection<PageDto>> GetAll(bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null, int? skip = null, int? take = null);
        Task<PageDto> GetById(int id, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null);
        Task<PageDto> GetByNodeId(int id, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null);
        Task UpdateCounts(CountDto dto);
    }

    public class PageManager : AssetManager, IPageManager
    {
        private readonly AppSettings _appSettings;

        public PageManager(IAssetRepository repository, INodeRepository nodeRepository, IKeywordRepository keywordRepository, IMediaRepository mediaRepository, IGalleryRepository galleryRepository, IUserRepository userRepository, ISocialNetworkRepository socialNetworkRepository, AppSettings appSettings)
            : base(repository, nodeRepository, galleryRepository, mediaRepository, keywordRepository, userRepository, socialNetworkRepository)
        {
            _appSettings = appSettings;
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<PageProfile>();
                cfg.AddProfile<NodeProfile>();
                cfg.AddProfile<KeywordProfile>();
                cfg.AddProfile<UpdateMediaProfile>();
                cfg.AddProfile<GalleryProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<PageDto>> GetAll(bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null, int? skip = null, int? take = null)
        {
            var pages = new List<PageDto>();

            var pageSet = (await _repository.Get(p => !p.IsDeleted)).OfType<Page>();

            if (skip.HasValue)
                pageSet = pageSet.OrderBy(p => p.Id).Skip(skip.Value);

            if (take.HasValue)
                pageSet = pageSet.Take(take.Value);

            foreach (var page in pageSet)
                pages.Add(MapToDto(page, includeContent, includeNodes, includeMedia, includeGalleries, languageId));

            return pages;
        }

        public async Task<PageDto> GetById(int id, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var page = (await _repository.Get(p => p.Id == id && !p.IsDeleted)).OfType<Page>().SingleOrDefault();
            return page != null ? MapToDto(page, includeContent, includeNodes, includeMedia, includeGalleries, languageId) : null;
        }

        public async Task<PageDto> GetByNodeId(int id, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var allPage = await _repository.Get(p => !p.IsDeleted && p.AssetNodes.Any(x => x.NodeId == id) && p is Page && !(p is News)); //(await _repository.Get(p => p.Nodes.Select(x => x.Id == id).Any() && !p.IsDeleted)).Include("Nodes").OfType<Page>().SingleOrDefault();
            var page = allPage.SingleOrDefault();
            return page != null ? MapToDto(page, includeContent, includeNodes, includeMedia, includeGalleries, languageId) : null;
        }

        public async Task<ICollection<PageDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(false, false, false, false, null, skip, take);
        }

        public async Task<PageDto> GetById(int id)
        {
            return await GetById(id, false, false, false, false, null);
        }

        public async Task<PageDto> Add(PageDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var page = (Page)await _repository.Add(await MapFromDto(dto));

            await SolrHelper.DataImport(SolrCore.PAGE, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);

            return MapToDto(page);
        }

        public async Task Update(PageDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var page = (await _repository.Get(p => p.Id == dto.Id && !p.IsDeleted)).OfType<Page>().SingleOrDefault();

            if (page == null)
                throw new EntityException("page");

            await _repository.Update(await MapFromDto(dto, page));

            await SolrHelper.DataImport(SolrCore.PAGE, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
        }

        public async Task Delete(PageDto dto)
        {
            var page = (await _repository.Get(p => p.Id == dto.Id && !p.IsDeleted)).Include(x => x.AssetNodes).OfType<Page>().SingleOrDefault();
            if (page == null)
                throw new EntityException("page");

            page.IsEnabled = false;
            page.IsDeleted = true;
            page.CacheSolr = false;

            await _repository.Update(page);
            await SolrHelper.DataImport(SolrCore.PAGE, _appSettings.Solr);

            var node = page?.AssetNodes?.FirstOrDefault();
            if (node != null)
            {
                node.Node.IsEnabled = false;
                await _nodeRepository.Update(node.Node);
                await SolrHelper.DataImport(SolrCore.NODE, _appSettings.Solr);
            }
        }

        public async Task UpdateCounts(CountDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var page = (await _repository.Get(p => p.Id == dto.Id && !p.IsDeleted)).OfType<Page>().SingleOrDefault();

            if (page == null)
                throw new EntityException("page");

            UpdateCount(dto, page);

            await _repository.Update(page);
            await SolrHelper.DataImport(SolrCore.PAGE, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private PageDto MapToDto(Asset page, bool includeContent = false, bool includeNodes = false, bool includeMedia = false, bool includeGalleries = false, int? languageId = null)
        {
            var dto = _Mapper.Map<PageDto>(page);

            if (includeContent)
            {
                foreach (var content in page.AssetContents.OfType<PageContent>().Where(c => !languageId.HasValue || c.LanguageId == languageId.Value))
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


            if (includeNodes)
            {
                foreach (var node in page.AssetNodes)
                {
                    var nodeDto = _Mapper.Map<DeleteNodeDtoBindingModel>(node.Node);
                    if (node.Node.NodeKeywords != null && node.Node.NodeKeywords.Any())
                        nodeDto.RelatedKeywords = string.Join(";", node.Node.NodeKeywords.Select(x => x.Keyword.Name).ToList());
                    dto.Nodes.Add(nodeDto);
                }
            }

            if (includeMedia)
            {
                foreach (var assetMedia in page.AssetMedia)
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
                            assetMediaDto.Media.Size1Path =  _appSettings.Content.AdminDomain + sizesPaths.Size1Path;
                            assetMediaDto.Media.Size2Path =  _appSettings.Content.AdminDomain + sizesPaths.Size2Path;
                            assetMediaDto.Media.Size3Path =  _appSettings.Content.AdminDomain + sizesPaths.Size3Path;
                            assetMediaDto.Media.Size4Path =  _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                            assetMediaDto.Media.Size5Path = _appSettings.Content.AdminDomain + sizesPaths.Size5Path;
                        }
                    }

                    dto.AssetMedia.Add(assetMediaDto);
                }
            }

            if (includeGalleries)
            {
                foreach (var gallery in page.Galleries)
                {
                    var galleryDto = _Mapper.Map<DeleteGalleryDtoBindingModel>(gallery);

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
                                    mediaDto.Size1Path =  _appSettings.Content.AdminDomain + sizesPaths.Size1Path;
                                    mediaDto.Size2Path =  _appSettings.Content.AdminDomain + sizesPaths.Size2Path;
                                    mediaDto.Size3Path =  _appSettings.Content.AdminDomain + sizesPaths.Size3Path;
                                    mediaDto.Size4Path =  _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                                    mediaDto.Size5Path = _appSettings.Content.AdminDomain + sizesPaths.Size5Path;
                                }
                            }

                            galleryDto.Media.Add(mediaDto);
                        }
                    }

                    dto.Galleries.Add(galleryDto);
                }
            }

            return dto;
        }

        private async Task<Page> MapFromDto(PageDto pageDto, Page page = null)
        {
            Page ret;

            if (page == null)
            {
                var newPage = _Mapper.Map<Page>(pageDto);

                if (pageDto.Nodes != null && pageDto.Nodes.Any())
                {
                    foreach (var nodeDto in pageDto.Nodes)
                    {
                        var node = await _nodeRepository.GetById(nodeDto.Id);

                        if (node == null)
                            throw new EntityException("node");

                        newPage.AssetNodes.Add(new AssetNode(){NodeId =  node.Id});
                    }
                }

                if (pageDto.AssetMedia != null && pageDto.AssetMedia.Any())
                {
                    foreach (var assetMediaDto in pageDto.AssetMedia)
                    {
                        var media = await _mediaRepository.GetById(assetMediaDto.Media.Id);

                        if (media == null)
                            throw new EntityException("media");

                        var assetMedia = new AssetMedia
                        {
                            Media = media,
                            Featured = assetMediaDto.Featured
                        };
                        newPage.AssetMedia.Add(assetMedia);
                    }
                }
                else
                {
                    if (page != null && page.AssetMedia != null && page.AssetMedia.Count > 0)
                    {
                        var listRemove = page.AssetMedia.ToList();
                        foreach (var item in listRemove)
                            page.AssetMedia.Remove(item);
                    }
                }

                if (pageDto.Galleries != null && pageDto.Galleries.Any())
                {
                    foreach (var galleryDto in pageDto.Galleries)
                    {
                        var gallery = await _galleryRepository.GetById(galleryDto.Id);

                        if (gallery == null)
                            throw new EntityException("gallery");

                        newPage.Galleries.Add(new AssetGallery(){GalleryId = gallery.Id});
                    }
                }

                ret = newPage;
            }
            else
            {
                _Mapper.Map<PageDto, Page>(pageDto, page);

                await UpdateNodes(pageDto, page);
                await UpdateMedia(pageDto, page);
                await UpdateGalleries(pageDto, page);

                ret = page;
            }

            if (pageDto.Content != null)
            {
                foreach (var content in pageDto.Content)
                {
                    var existingContent = ret.AssetContents.OfType<PageContent>().SingleOrDefault(c => c.LanguageId == content.LanguageId);

                    if (existingContent != null)
                    {
                        _Mapper.Map<PageContentDto, PageContent>(content, existingContent);

                        existingContent.BackgroundColor = content.BackgroundColor;

                        if (string.IsNullOrWhiteSpace(existingContent.MobileTitle))
                            existingContent.MobileTitle = existingContent.Title;

                        UpdateKeywords(pageDto, existingContent);
                        await UpdateSocialNetworks(pageDto, existingContent);
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
                                var keyword = (await _keywordRepository.Get(k => k.Name.Equals(sKeyword) && k.IsEnabled)).SingleOrDefault();

                                if (keyword == null)
                                    keyword = new Keyword(sKeyword);

                                newContent.PageContentKeywords.Add(new PageContentKeyword(){Keyword = keyword});
                                newContent.Keywords = string.IsNullOrWhiteSpace(newContent.Keywords) ? sKeyword : string.Format("{0},{1}", newContent.Keywords, sKeyword);
                            }
                        }

                        if (content.SocialNetwork.Any() && content.SocialNetwork != null)
                        {
                            foreach (var snDto in content.SocialNetwork)
                            {
                                var socialNetwork = await _socialNetworkRepository.GetById(snDto.Id);

                                if (socialNetwork == null || !socialNetwork.IsEnabled)
                                    throw new EntityException("social network");

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

            return ret;
        }
    }
}
