using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Core;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Configuration;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface IChannelManager : IManager<ChannelDto>
    {
    }

    public class ChannelManager : BaseManager, IChannelManager
    {
        private readonly IChannelRepository _repository;
        private readonly AppSettings _appSettings;

        public ChannelManager(IChannelRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Channel, ChannelDto>()
                    .ForMember(x => x.Media, opt => opt.Ignore());

                cfg.CreateMap<ChannelDto, Channel>()
                    .ForMember(x => x.Media, opt => opt.Ignore());
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<ChannelDto>> GetAll(int? skip = null, int? take = null)
        {
            var channels = new List<ChannelDto>();

            var channelSet = await _repository.Get(p => !p.IsDeleted);

            if (skip.HasValue)
                channelSet = channelSet.OrderBy(g => g.Id).Skip(skip.Value);

            if (take.HasValue)
                channelSet = channelSet.Take(take.Value);

            foreach (var channel in channelSet)
                channels.Add(MapToDto(channel));

            return channels;
        }

        public async Task<ChannelDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var channel = await _repository.GetById(id);
            return channel != null && !channel.IsDeleted ? MapToDto(channel) : null;
        }

        public async Task<ChannelDto> Add(ChannelDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("channel");

            var existingChannel = await _repository.Get(g => g.Name.ToLower().Equals(dto.Name.ToLower()) && !g.IsDeleted);

            if (existingChannel.Any())
                throw new AlreadyExistsException("channel");

            var entityToCreate = MapFromDto(dto);
            entityToCreate.CacheSolr = false;

            if (dto.Media != null && dto.Media.Id > 0)
                entityToCreate.MediaId =dto.Media.Id ;
            else
                entityToCreate.MediaId = null;

            var channel = await _repository.Add(entityToCreate);
            
            await SolrHelper.DataImport(SolrCore.CHANNEL, _appSettings.Solr);

            return MapToDto(channel);
        }

        public async Task Update(ChannelDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("channel");

            var channel = await _repository.GetById(dto.Id);

            if (channel == null)
                throw new ItemNotFoundException("channel");

            var existingProgrammingGuide = await _repository.Get(g => g.Name.ToLower().Equals(dto.Name.ToLower()) && g.Id != dto.Id && !g.IsDeleted);

            if (existingProgrammingGuide.Any())
                throw new AlreadyExistsException("channel");

            //Pass creation data because is null
            dto.CreationDate = channel.CreationDate;
            dto.CreationUser = channel.CreationUser;

            if (string.IsNullOrEmpty(dto.Name))
                dto.Name = channel.Name;
            else if (channel.Name != dto.Name)
                channel.Name = dto.Name;

            var entityToUpdate = MapFromDto(dto, channel);
            
            if (dto.Media != null && dto.Media.Id > 0)
                entityToUpdate.MediaId =dto.Media.Id;
            
            entityToUpdate.CacheSolr = false;
            await _repository.Update(entityToUpdate);
            
            await SolrHelper.DataImport(SolrCore.CHANNEL, _appSettings.Solr);
        }

        public async Task Delete(ChannelDto dto)
        {
            var channel = await _repository.GetById(dto.Id);

            if (channel == null || channel.IsDeleted)
                throw new DeleteException("channel");

            channel.IsDeleted = true;
            channel.CacheSolr = false;

            await _repository.Update(channel);

            await SolrHelper.DataImport(SolrCore.CHANNEL, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private ChannelDto MapToDto(Channel channel)
        {
            var dto = _Mapper.Map<ChannelDto>(channel);

            if (channel.Media != null)
            {
                var mediaDto = new MediaDto
                {
                    Id = channel.Media.Id
                };

                if (!string.IsNullOrWhiteSpace(channel.Media.SizesPaths))
                {
                    var sizesPaths = JsonConvert.DeserializeObject<dynamic>(channel.Media.SizesPaths);
                    if (sizesPaths != null)
                    {
                        mediaDto.Size1Path =  _appSettings.Content.AdminDomain + sizesPaths.Size1Path;
                        mediaDto.Size2Path =  _appSettings.Content.AdminDomain + sizesPaths.Size2Path;
                        mediaDto.Size3Path =  _appSettings.Content.AdminDomain + sizesPaths.Size3Path;
                        mediaDto.Size4Path =  _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                        mediaDto.Size5Path = _appSettings.Content.AdminDomain + sizesPaths.Size5Path;
                    }
                }

                dto.Media = mediaDto;
            }
            return dto;
        }

        private Channel MapFromDto(ChannelDto channelDto, Channel channel = null)
        {
            var dto = channel == null ? _Mapper.Map<Channel>(channelDto) : _Mapper.Map<ChannelDto, Channel>(channelDto, channel);
            return dto;
        }
    }
}
