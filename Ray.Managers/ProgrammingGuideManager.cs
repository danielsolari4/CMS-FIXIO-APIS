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
    public interface IProgrammingGuideManager : IManager<ProgrammingGuideDto>
    {
        Task<ICollection<ProgrammingGuideDto>> GetAll(int? skip, int? take, bool includeMedia = false, bool includeNodes = false, bool includeChannels = false);
        Task<ProgrammingGuideDto> GetById(int id, bool includeMedia = false, bool includeNodes = false, bool includeChannels = false);
    }

    public class ProgrammingGuideManager : BaseManager, IProgrammingGuideManager
    {
        private readonly IProgrammingGuideRepository _repository;
        private readonly IMediaRepository _mediaRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly ProgrammingGuideScheduleRepository _ProgrammingGuideSchedulesRepository;
        private readonly IProgramGalleryRepository _programGalleryRepository;
        private readonly AppSettings _appSettings;

        public ProgrammingGuideManager(IProgrammingGuideRepository repository, IMediaRepository mediaRepository, INodeRepository nodeRepository, ProgrammingGuideScheduleRepository ProgrammingGuideSchedulesRepository, IProgramGalleryRepository programGalleryRepository, AppSettings appSettings)
        {
            _repository = repository;
            _mediaRepository = mediaRepository;
            _nodeRepository = nodeRepository;
            _ProgrammingGuideSchedulesRepository = ProgrammingGuideSchedulesRepository;
            _programGalleryRepository = programGalleryRepository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<ProgrammingGuideProfile>();
                cfg.AddProfile<CreateMediaProfile>();
                cfg.AddProfile<NodeProfile>();
                cfg.AddProfile<KeywordProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<ProgrammingGuideDto>> GetAll(int? skip, int? take, bool includeMedia = false, bool includeNodes = false, bool includeChannels = false)
        {
            var programmingGuides = new List<ProgrammingGuideDto>();

            var programmingGuideSet = await _repository.Get(p => !p.IsDeleted);

            if (skip.HasValue)
                programmingGuideSet = programmingGuideSet.OrderBy(g => g.Id).Skip(skip.Value);

            if (take.HasValue)
                programmingGuideSet = programmingGuideSet.Take(take.Value);

            foreach (var programmingGuide in programmingGuideSet)
                programmingGuides.Add(MapToDto(programmingGuide, includeMedia, includeNodes, includeChannels));

            return programmingGuides;
        }

        public async Task<ProgrammingGuideDto> GetById(int id, bool includeMedia = false, bool includeNodes = false, bool includeChannels = false)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var programmingGuide = await _repository.GetById(id);

            if (programmingGuide != null && !programmingGuide.IsDeleted)
                return MapToDto(programmingGuide, includeMedia, includeNodes, includeChannels);

            return null;
        }

        public async Task<ICollection<ProgrammingGuideDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(skip, take, false);
        }

        public async Task<ProgrammingGuideDto> GetById(int id)
        {
            return await GetById(id, false);
        }

        public async Task<ProgrammingGuideDto> Add(ProgrammingGuideDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var existingProgrammingGuide = await _repository.Get(g => g.ProgramName.ToLower().Equals(dto.ProgramName.ToLower()) && !g.IsDeleted);

            if (existingProgrammingGuide.Any())
                throw new AlreadyExistsException("program");

            var program = await MapFromDto(dto);
            var order = 1;
            if (dto != null && dto.Galleries != null)
                foreach (var item in dto.Galleries)
                {
                    program.ProgramGalleries.Add(new ProgramGallery { GalleryId = item.Id, Order = order });
                    order++;
                }

            program.Featured = dto.Featured;

            var programmingGuide = await _repository.Add(program);

            await SolrHelper.DataImport(SolrCore.PROGRAMMINGGUIDE, _appSettings.Solr);


            return MapToDto(programmingGuide);
        }

        public async Task Update(ProgrammingGuideDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var programmingGuide = await _repository.GetById(dto.Id);

            if (programmingGuide == null)
                throw new EntityException("programmingGuide");

            var existingProgrammingGuide = await _repository.Get(g => g.ProgramName.ToLower().Equals(dto.ProgramName.ToLower()) && g.Id != dto.Id && !g.IsDeleted);

            if (existingProgrammingGuide.Any())
                throw new AlreadyExistsException("program");
            var program = await MapFromDto(dto, programmingGuide);

            if (dto.Node != null && dto.Node.Id > 0)
                program.NodeId = dto.Node.Id;
            else
                program.NodeId = null;

            var listToDelete = new List<ProgramGallery>();
            foreach (var it in program.ProgramGalleries)
                listToDelete.Add(it);
            foreach (var it in listToDelete)
                await _programGalleryRepository.Delete(it);

            await _repository.Update(program);

            var order = 1;
            if (dto.Galleries != null)
            {
                foreach (var item in dto.Galleries.OrderBy(x => x.Order))
                {
                    program.ProgramGalleries.Add(new ProgramGallery { GalleryId = item.Id, Order = order });
                    order++;
                }
            }

            program.Featured = dto.Featured;
            await _repository.Update(program);
            await SolrHelper.DataImport(SolrCore.PROGRAMMINGGUIDE, _appSettings.Solr);
        }

        public async Task Delete(ProgrammingGuideDto dto)
        {
            var programmingGuide = await _repository.GetById(dto.Id);

            if (programmingGuide == null || programmingGuide.IsDeleted)
                throw new EntityException("programmingGuide");

            programmingGuide.IsEnabled = false;
            programmingGuide.IsDeleted = true;
            programmingGuide.CacheSolr = false;

            await _repository.Update(programmingGuide);
            await SolrHelper.DataImport(SolrCore.PROGRAMMINGGUIDE, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private ProgrammingGuideDto MapToDto(ProgrammingGuide programmingGuide, bool includeMedia = false, bool includeNodes = false, bool includeChannels = false)
        {
            var dto = _Mapper.Map<ProgrammingGuideDto>(programmingGuide);

            foreach (var schedule in programmingGuide.ProgrammingGuideSchedules)
            {
                dto.Schedule.Add(_Mapper.Map<ProgrammingGuideScheduleDto>(schedule));
            }

            if (programmingGuide.NodeId != null && programmingGuide.NodeId > 0)
                dto.Node = new NodeDto
                {
                    Id = programmingGuide.NodeId ?? 0
                };

            dto.Galleries = new List<GalleryDto>();
            foreach (var gal in programmingGuide.ProgramGalleries.OrderBy(x => x.Order))
                if (gal.Gallery != null)
                    dto.Galleries.Add(new GalleryDto
                    {
                        Id = gal.Gallery.Id,
                        Name = gal.Gallery.Name,
                        CreationDate = gal.Gallery.CreationDate,
                        IsEnabled = gal.Gallery.IsEnabled,
                        Order = gal.Order
                    });

            if (includeMedia)
            {
                foreach (var it in programmingGuide.ProgrammingGuideMedia)
                {
                    var mediaDto = _Mapper.Map<MediaDto>(it.Media);

                    if (!string.IsNullOrWhiteSpace(it.Media.SizesPaths))
                    {
                        var sizesPaths = JsonConvert.DeserializeObject<dynamic>(it.Media.SizesPaths);
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
            }
            if (includeNodes)
            {
                foreach (var node in programmingGuide.ProgrammingGuideNodes)
                {
                    dto.Nodes.Add(_Mapper.Map<DeleteNodeDtoBindingModel>(node.Node));
                }
            }

            #region Channel map information

            if (includeChannels)
            {
                dto.Channel = programmingGuide.ChannelNavigation.Name;
                dto.ChannelId = programmingGuide.ChannelNavigation.Id;
            }

            #endregion

            return dto;
        }

        private async Task<ProgrammingGuide> MapFromDto(ProgrammingGuideDto programmingGuideDto, ProgrammingGuide programmingGuide = null)
        {
            ProgrammingGuide ret;

            if (programmingGuide == null)
            {
                var newProgrammingGuide = _Mapper.Map<ProgrammingGuide>(programmingGuideDto);

                if (programmingGuideDto.Media != null && programmingGuideDto.Media.Any())
                {
                    foreach (var m in programmingGuideDto.Media)
                    {
                        var media = await _mediaRepository.GetById(m.Id);

                        if (media == null)
                            throw new EntityException("media");

                        newProgrammingGuide.ProgrammingGuideMedia.Add(new ProgrammingGuideMedia() { MediaId = media.Id });
                    }
                }

                if (programmingGuideDto.Nodes != null && programmingGuideDto.Nodes.Any())
                {
                    foreach (var nodeDto in programmingGuideDto.Nodes)
                    {
                        var node = await _nodeRepository.GetById(nodeDto.Id);

                        if (node == null)
                            throw new EntityException("node");

                        newProgrammingGuide.ProgrammingGuideNodes.Add(new ProgrammingGuideNode() { NodeId = node.Id });
                    }
                }

                if (programmingGuideDto.Schedule != null)
                {
                    foreach (var schedule in programmingGuideDto.Schedule)
                    {
                        var newSchedule = _Mapper.Map<ProgrammingGuideSchedule>(schedule);
                        newProgrammingGuide.ProgrammingGuideSchedules.Add(newSchedule);
                    }
                }

                ret = newProgrammingGuide;
            }
            else
            {
                _Mapper.Map<ProgrammingGuideDto, ProgrammingGuide>(programmingGuideDto, programmingGuide);

                await UpdateMedia(programmingGuideDto, programmingGuide);
                await UpdateNodes(programmingGuideDto, programmingGuide);
                await UpdateSchedule(programmingGuideDto, programmingGuide);

                ret = programmingGuide;
            }

            ret.CacheSolr = false;

            return ret;
        }

        private async Task UpdateMedia(ProgrammingGuideDto programmingGuideDto, ProgrammingGuide programmingGuide)
        {
            var mediaToRemove = programmingGuide.ProgrammingGuideMedia.Where(media => !programmingGuideDto.Media.Any(m => m.Id == media.MediaId)).ToArray();

            if (mediaToRemove != null && mediaToRemove.Any())
                foreach (var media in mediaToRemove)
                    programmingGuide.ProgrammingGuideMedia.Remove(media);

            if (programmingGuideDto.Media != null && programmingGuideDto.Media.Any())
                foreach (var mediaDto in programmingGuideDto.Media)
                {
                    var programmingGuideMedia = programmingGuide.ProgrammingGuideMedia.FirstOrDefault(h => h.MediaId == mediaDto.Id);

                    if (programmingGuideMedia == null)
                    {
                        var media = await _mediaRepository.GetById(mediaDto.Id);

                        if (media == null)
                            throw new EntityException("media");

                        programmingGuide.ProgrammingGuideMedia.Add(new ProgrammingGuideMedia() { MediaId = media.Id });
                    }
                }
        }



        private async Task UpdateNodes(ProgrammingGuideDto programmingGuideDto, ProgrammingGuide programmingGuide)
        {
            var nodesToRemove = programmingGuide.ProgrammingGuideNodes.Where(node => !programmingGuideDto.Nodes.Any(n => n.Id == node.NodeId)).ToArray();

            if (nodesToRemove != null && nodesToRemove.Any())
                foreach (var node in nodesToRemove)
                    programmingGuide.ProgrammingGuideNodes.Remove(node);

            var nodesToAdd = programmingGuideDto.Nodes.Where(nodes => !programmingGuide.ProgrammingGuideNodes.Any(n => n.NodeId == nodes.Id));

            if (nodesToAdd != null && nodesToAdd.Any())
                foreach (var nodeDto in nodesToAdd)
                {
                    var node = await _nodeRepository.GetById(nodeDto.Id);

                    if (node == null)
                        throw new EntityException("node");

                    programmingGuide.ProgrammingGuideNodes.Add(new ProgrammingGuideNode() { NodeId = node.Id });
                }
        }
        private async Task UpdateSchedule(ProgrammingGuideDto programmingGuideDto, ProgrammingGuide programmingGuide)
        {
            var schedulesToRemove = programmingGuide.ProgrammingGuideSchedules.Where(schedule => !programmingGuideDto.Schedule.Any(s => s.Day == schedule.Day && s.InitHour == schedule.InitHour && s.InitMinute == schedule.InitMinute && s.EndHour == schedule.EndHour && s.EndMinute == schedule.EndMinute)).ToArray();

            if (schedulesToRemove != null && schedulesToRemove.Any())
                foreach (var schedule in schedulesToRemove)
                {
                    programmingGuide.ProgrammingGuideSchedules.Remove(schedule);
                    await _ProgrammingGuideSchedulesRepository.Delete(schedule);
                }

            var schedulesToAdd = programmingGuideDto.Schedule.Where(schedule => !programmingGuide.ProgrammingGuideSchedules.Any(s => s.Day == schedule.Day && s.InitHour == schedule.InitHour && s.InitMinute == schedule.InitMinute && s.EndHour == schedule.EndHour && s.EndMinute == schedule.EndMinute));

            if (schedulesToAdd != null && schedulesToAdd.Any())
                foreach (var schedule in schedulesToAdd)
                {
                    var newSchedule = _Mapper.Map<ProgrammingGuideSchedule>(schedule);

                    programmingGuide.ProgrammingGuideSchedules.Add(newSchedule);
                }
        }

    }
}
