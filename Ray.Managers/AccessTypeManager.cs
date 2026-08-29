using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Core;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface IAccessTypeManager : IManager<AccessTypeDto>
    {
    }

    public class AccessTypeManager : BaseManager, IAccessTypeManager
    {
        private readonly IAccessTypeRepository _repository;
        private readonly AppSettings _appSettings;

        public AccessTypeManager(IAccessTypeRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.CreateMap<AccessTypeDto, AccessType>();
                cfg.CreateMap<AccessType, AccessTypeDto>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<AccessTypeDto>> GetAll(int? skip = null, int? take = null)
        {
            var AccessTypes = new List<AccessTypeDto>();

            var AccessTypeSet = await _repository.GetAll();

            if (skip.HasValue)
                AccessTypeSet = AccessTypeSet.OrderBy(g => g.Id).Skip(skip.Value);

            if (take.HasValue)
                AccessTypeSet = AccessTypeSet.Take(take.Value);

            foreach (var AccessType in AccessTypeSet)
                AccessTypes.Add(MapToDto(AccessType));

            return AccessTypes;
        }

        public async Task<AccessTypeDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var AccessType = await _repository.GetById(id);
            return AccessType != null ? MapToDto(AccessType) : null;
        }

        public async Task<AccessTypeDto> Add(AccessTypeDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("AccessType");

            var existingAccessType = await _repository.Get(g => g.Name.ToLower().Equals(dto.Name.ToLower()));

            if (existingAccessType.Any())
                throw new AlreadyExistsException("AccessType");

            var entityToCreate = MapFromDto(dto);

            var AccessType = await _repository.Add(entityToCreate);

            await SolrHelper.DataImport(SolrCore.ACCESSTYPE, _appSettings.Solr);

            return MapToDto(AccessType);
        }

        public async Task Update(AccessTypeDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("AccessType");

            var AccessType = await _repository.GetById(dto.Id);

            if (AccessType == null)
                throw new ItemNotFoundException("AccessType");

            var existingProgrammingGuide = await _repository.Get(g => g.Name.ToLower().Equals(dto.Name.ToLower()) && g.Id != dto.Id);

            if (existingProgrammingGuide.Any())
                throw new AlreadyExistsException("AccessType");

            
            if (string.IsNullOrEmpty(dto.Name))
                dto.Name = AccessType.Name;
            else if (AccessType.Name != dto.Name)
                AccessType.Name = dto.Name;

            var entityToUpdate = MapFromDto(dto, AccessType);
            

            await _repository.Update(entityToUpdate);

            await SolrHelper.DataImport(SolrCore.ACCESSTYPE, _appSettings.Solr);
        }

        public async Task Delete(AccessTypeDto dto)
        {
            var AccessType = await _repository.GetById(dto.Id);

            if (AccessType == null)
                throw new DeleteException("AccessType");

            await _repository.Update(AccessType);

            await SolrHelper.DataImport(SolrCore.ACCESSTYPE, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private AccessTypeDto MapToDto(AccessType AccessType)
        {
            var dto = _Mapper.Map<AccessTypeDto>(AccessType);
            return dto;
        }

        private AccessType MapFromDto(AccessTypeDto AccessTypeDto, AccessType AccessType = null)
        {
            return AccessType == null ? _Mapper.Map<AccessType>(AccessTypeDto) : _Mapper.Map<AccessTypeDto, AccessType>(AccessTypeDto, AccessType);
        }
    }
}
