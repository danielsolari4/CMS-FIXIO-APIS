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
    public interface IGalleryManager : IManager<GalleryDto>
    {
        Task<ICollection<GalleryDto>> GetAll(int? skip, int? take, bool includeMedia = false);
        Task<GalleryDto> GetById(int id, bool includeMedia = false);
    }

    public class GalleryManager : BaseManager, IGalleryManager
    {
        private readonly IGalleryRepository _repository;
        private readonly IMediaRepository _mediaRepository;
        private readonly AppSettings _appSettings;

        public GalleryManager(IGalleryRepository repository, IMediaRepository mediaRepository, AppSettings appSettings)
        {
            _repository = repository;
            _mediaRepository = mediaRepository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<GalleryProfile>();
                cfg.AddProfile<CreateMediaProfile>();
                cfg.AddProfile<KeywordProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<GalleryDto>> GetAll(int? skip, int? take, bool includeMedia = false)
        {
            var galleries = new List<GalleryDto>();

            var gallerySet = await _repository.GetAll();

            if (skip.HasValue)
                gallerySet = gallerySet.OrderBy(g => g.Id).Skip(skip.Value);

            if (take.HasValue)
                gallerySet = gallerySet.Take(take.Value);

            foreach (var gallery in gallerySet)
                galleries.Add(MapToDto(gallery, includeMedia));

            return galleries;
        }

        public async Task<GalleryDto> GetById(int id, bool includeMedia = false)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var gallery = await _repository.GetById(id);
            return gallery != null ? MapToDto(gallery, includeMedia) : null;
        }

        public async Task<ICollection<GalleryDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(skip, take, false);
        }

        public async Task<GalleryDto> GetById(int id)
        {
            return await GetById(id, false);
        }

        public async Task<GalleryDto> Add(GalleryDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            //if ((await _repository.Get(g => g.Name.Equals(dto.Name))).Any())
            //    throw new ArgumentException("gallery already exists"); //We do not allow two galleries with the same name.

            var gallery = await _repository.Add(await MapFromDto(dto));
            await SolrHelper.DataImport(SolrCore.GALLERY, _appSettings.Solr);

            return MapToDto(gallery);
        }

        public async Task Update(GalleryDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var gallery = await _repository.GetById(dto.Id);

            if (gallery == null)
                throw new ItemNotFoundException("gallery");

            //if ((await _repository.Get(g => g.Name.Equals(dto.Name) && g.Id != dto.Id)).Any())
            //    throw new ArgumentException("gallery already exists"); //We do not allow two galleries with the same name.

            await _repository.Update(await MapFromDto(dto, gallery));
            await SolrHelper.DataImport(SolrCore.GALLERY, _appSettings.Solr);
        }

        public async Task Delete(GalleryDto dto)
        {
            var gallery = await _repository.GetById(dto.Id);

            if (gallery == null)
                throw new DeleteException("gallery");

            // Clears Media-Keyword relations
            gallery.MediaGalleries.Clear();

            await _repository.Delete(gallery);
            await SolrHelper.DeleteDocumentById(SolrCore.GALLERY, gallery.Id, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private GalleryDto MapToDto(Gallery gallery, bool includeMedia = false)
        {
            var dto = _Mapper.Map<GalleryDto>(gallery);

            if (includeMedia)
            {
                foreach (var media in gallery.MediaGalleries)
                {
                    var mediaDto = _Mapper.Map<MediaDto>(media.Media);

                    if (media.Order.HasValue)
                        mediaDto.Order = media.Order.Value;

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

                    dto.Media.Add(mediaDto);
                }

                if (dto.Media.Any())
                    dto.Media = dto.Media.OrderBy(m => m.Order).ToList();
            }

            return dto;
        }

        private async Task<Gallery> MapFromDto(GalleryDto galleryDto, Gallery gallery = null)
        {
            Gallery ret;

            if (gallery == null)
            {
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

                ret = newGallery;
            }
            else
            {
                _Mapper.Map<GalleryDto, Gallery>(galleryDto, gallery);

                await UpdateMedia(galleryDto, gallery);

                ret = gallery;
            }

            ret.CacheSolr = false;

            return ret;
        }

        private async Task UpdateMedia(GalleryDto galleryDto, Gallery gallery)
        {
            var mediaToRemove = gallery.MediaGalleries.Where(media => !galleryDto.Media.Any(m => m.Id == media.MediaId)).ToArray();

            if (mediaToRemove != null && mediaToRemove.Any())
                foreach (var media in mediaToRemove)
                    gallery.MediaGalleries.Remove(media);

            if (galleryDto.Media != null && galleryDto.Media.Any())
                foreach (var mediaDto in galleryDto.Media)
                {
                    var mediaGallery = gallery.MediaGalleries.FirstOrDefault(h => h.MediaId == mediaDto.Id);

                    if (mediaGallery == null)
                    {
                        var media = await _mediaRepository.GetById(mediaDto.Id);

                        if (media == null)
                            throw new ArgumentNullException("media");

                        var mg = new MediaGallery
                        {
                            Media = media,
                            Order = mediaDto.Order
                        };
                        gallery.MediaGalleries.Add(mg);
                    }
                    else
                        mediaGallery.Order = mediaDto.Order;
                }
        }
    }
}
