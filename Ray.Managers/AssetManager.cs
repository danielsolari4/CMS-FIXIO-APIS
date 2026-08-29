using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ray.Dtos;
using Ray.Managers.Core;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;

namespace Ray.Managers
{
    public abstract class AssetManager : BaseManager
    {
        protected readonly IAssetRepository _repository;
        protected readonly IGalleryRepository _galleryRepository;
        protected readonly INodeRepository _nodeRepository;
        protected readonly IMediaRepository _mediaRepository;
        protected readonly IKeywordRepository _keywordRepository;
        protected readonly IUserRepository _userRepository;
        protected readonly ISocialNetworkRepository _socialNetworkRepository;

        protected AssetManager(IAssetRepository repository, INodeRepository nodeRepository, IGalleryRepository galleryRepository, IMediaRepository mediaRepository, IKeywordRepository keywordRepository, IUserRepository userRepository, ISocialNetworkRepository socialNetworkRepository)
        {
            _repository = repository;
            _nodeRepository = nodeRepository;
            _galleryRepository = galleryRepository;
            _mediaRepository = mediaRepository;
            _keywordRepository = keywordRepository;
            _userRepository = userRepository;
            _socialNetworkRepository = socialNetworkRepository;
        }

        protected async Task UpdateGalleries(AssetDto assetDto, Asset asset)
        {
            var galleriesToRemove = asset.Galleries.Where(gallery => !assetDto.Galleries.Any(galleryDto => galleryDto.Id == gallery.GalleryId)).ToArray();

            if (galleriesToRemove != null && galleriesToRemove.Any())
                foreach (var gallery in galleriesToRemove)
                    asset.Galleries.Remove(gallery);

            var galleriesToAdd = assetDto.Galleries.Where(galleryDto => !asset.Galleries.Any(gallery => gallery.GalleryId == galleryDto.Id));

            if (galleriesToAdd != null && galleriesToAdd.Any())
                foreach (var galleryDto in galleriesToAdd)
                {
                    var gallery = await _galleryRepository.GetById(galleryDto.Id);

                    if (gallery == null)
                        throw new ArgumentNullException("gallery");

                    asset.Galleries.Add(new AssetGallery() { GalleryId = gallery.Id });
                }
        }

        protected async Task UpdateNodes(AssetDto assetDto, Asset asset)
        {
            var nodesToRemove = asset.AssetNodes.Where(node => !assetDto.Nodes.Any(n => n.Id == node.NodeId)).ToArray();

            if (nodesToRemove != null && nodesToRemove.Any())
                foreach (var node in nodesToRemove)
                    asset.AssetNodes.Remove(node);

            var nodesToAdd = assetDto.Nodes.Where(nodes => !asset.AssetNodes.Any(n => n.NodeId == nodes.Id));

            if (nodesToAdd != null && nodesToAdd.Any())
                foreach (var nodeDto in nodesToAdd)
                {
                    var node = await _nodeRepository.GetById(nodeDto.Id);

                    if (node == null)
                        throw new ArgumentNullException("nodes");

                    asset.AssetNodes.Add(new AssetNode { NodeId = node.Id });
                }
        }

        protected async Task UpdateMedia(AssetDto assetDto, Asset asset)
        {
            var mediaToRemove = asset.AssetMedia.Where(am => !assetDto.AssetMedia.Any(amdto => amdto.Media.Id == am.MediaId)).ToArray();

            if (mediaToRemove != null && mediaToRemove.Any())
                foreach (var am in mediaToRemove)
                    asset.AssetMedia.Remove(am);

            var mediaToAdd = assetDto.AssetMedia.Where(amdto => !asset.AssetMedia.Any(am => am.MediaId == amdto.Media.Id));

            if (mediaToAdd != null && mediaToAdd.Any())
                foreach (var assetMediaDto in mediaToAdd)
                {
                    var media = await _mediaRepository.GetById(assetMediaDto.Media.Id);

                    if (media == null)
                        throw new ArgumentNullException("media");

                    var assetMedia = new AssetMedia
                    {
                        Media = media,
                        Featured = assetMediaDto.Featured
                    };
                    asset.AssetMedia.Add(assetMedia);
                }
        }

        protected async void UpdateKeywords(PageDto pageDto, PageContent existingContent)
        {
            foreach (var contentDto in pageDto.Content)
            {
                var keywords = !string.IsNullOrWhiteSpace(contentDto.Keywords) ? contentDto.Keywords.Split(',').Select(k => k.Trim().ToLower()) : new List<string>();
                existingContent.Keywords = string.Empty;

                foreach (var sKeyword in keywords)
                {
                    var keywordList = await _keywordRepository.Get(k => k.Name.Equals(sKeyword) && k.IsEnabled);
                    var keyword = keywordList.FirstOrDefault();

                    if (keyword == null)
                        existingContent.PageContentKeywords.Add(new PageContentKeyword(){Keyword = new Keyword(sKeyword) });
                    else
                    {
                        if (!existingContent.PageContentKeywords.Any(k => k.Keyword.Name.Equals(keyword.Name)))
                            existingContent.PageContentKeywords.Add(new PageContentKeyword() { Keyword = keyword});
                    }

                    existingContent.Keywords = string.IsNullOrWhiteSpace(existingContent.Keywords) ? sKeyword : string.Format("{0},{1}", existingContent.Keywords, sKeyword);
                }

                foreach (var keyword in existingContent.PageContentKeywords.Where(keyword => !keywords.Any(sKeyword => keyword.Keyword.Name.Equals(sKeyword))).ToArray())
                {
                    existingContent.PageContentKeywords.Remove(keyword);
                }
            }

        }

        protected async Task UpdateSocialNetworks(PageDto pageDto, PageContent existingContent)
        {
            foreach (var contentDto in pageDto.Content)
            {
                if (existingContent.PageContentSocialNetworks.Any() && existingContent.PageContentSocialNetworks != null)
                {
                    var existingSocialNetworks = existingContent.PageContentSocialNetworks.Where(pc => contentDto.SocialNetwork.Any(snDto => snDto.Id == pc.SocialNetworkId)).ToArray();

                    if (existingSocialNetworks.Any() && existingSocialNetworks != null)
                        foreach (var socialNetwork in existingSocialNetworks)
                        {
                            var socialNetworkDto = contentDto.SocialNetwork.FirstOrDefault(sn => sn.Id == socialNetwork.SocialNetworkId);

                            if (socialNetwork.Url != socialNetworkDto.Url)
                                socialNetwork.Url = socialNetworkDto.Url;
                        }
                }

                var socialNetworksToRemove = existingContent.PageContentSocialNetworks.Where(pc => !contentDto.SocialNetwork.Any(snDto => snDto.Id == pc.SocialNetworkId)).ToArray();

                if (socialNetworksToRemove != null && socialNetworksToRemove.Any())
                    foreach (var sn in socialNetworksToRemove)
                        existingContent.PageContentSocialNetworks.Remove(sn);

                var socialNetworksToAdd = contentDto.SocialNetwork.Where(snDto => !existingContent.PageContentSocialNetworks.Any(pc => pc.SocialNetworkId == snDto.Id));

                if (socialNetworksToAdd != null && socialNetworksToAdd.Any())
                    foreach (var socialNetworkDto in socialNetworksToAdd)
                    {
                        var socialNetwork = await _socialNetworkRepository.GetById(socialNetworkDto.Id);

                        if (socialNetwork == null || !socialNetwork.IsEnabled)
                            throw new ArgumentNullException("social_network");

                        var pageContentSocialNetwork = new PageContentSocialNetwork
                        {
                            SocialNetwork = socialNetwork,
                            Url = socialNetworkDto.Url
                        };
                        existingContent.PageContentSocialNetworks.Add(pageContentSocialNetwork);
                    }
            }
        }

        protected void UpdateCount(CountDto countDto, Page page)
        {
            switch (countDto.Discriminator)
            {
                case CountDiscriminator.Share:
                    if (countDto.Count != 0)
                    {
                        if (page.ShareCount > 0)
                            page.ShareCount = page.ShareCount + countDto.Count;
                        else
                            page.ShareCount = countDto.Count;
                    }
                    else
                        throw new ArgumentNullException("count");
                    break;

                case CountDiscriminator.Views:
                    if (countDto.Count != 0)
                    {
                        if (page.ViewsCount > 0)
                            page.ViewsCount = page.ViewsCount + countDto.Count;
                        else
                            page.ViewsCount = countDto.Count;
                    }
                    else
                        throw new ArgumentNullException("count");
                    break;

                default:
                    throw new ArgumentNullException("discriminator");
            }
        }
    }
}
