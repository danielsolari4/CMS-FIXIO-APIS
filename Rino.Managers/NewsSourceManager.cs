using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers.Core;
using Rino.Model.NewContext.Entities;
using Rino.Repositories;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface INewsSourceManager : IManager<NewsSourceDto>
    {
    }

    public class NewsSourceManager : BaseManager, INewsSourceManager
    {
        private readonly INewsSourceRepository _repository;
        private readonly AppSettings _appSettings;

        public NewsSourceManager(INewsSourceRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;
        }

        public async Task<NewsSourceDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("Id");
            var entity = await _repository.GetById(id);

            if (entity == null)
                return null;

            return new NewsSourceDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public async Task<NewsSourceDto> Add(NewsSourceDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var entity = new NewsSource
            {
                Name = dto.Name,
                //Structure = JsonConvert.SerializeObject(dto.Name)
            };

            var exists = _repository.GetAll().Result.FirstOrDefault(x => x.Id == dto.Id);

            if (exists != null)
            {
                //exists.Structure = entity.Structure;
                await _repository.Update(exists);

                dto.Id = exists.Id;
                return dto;
            }

            var result = await _repository.Add(entity);
            var json = JsonConvert.SerializeObject(result);
            return JsonConvert.DeserializeObject<NewsSourceDto>(json);

        }

        public async Task Update(NewsSourceDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var node = await _repository.GetById(dto.Id);
        }
        public async Task Delete(NewsSourceDto dto)
        {
            var node = await _repository.GetById(dto.Id);

            await _repository.Update(node);
            await SolrHelper.DataImport(SolrCore.NODE, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        public async Task<ICollection<NewsSourceDto>> GetAll(int? skip = null, int? take = null)
        {
            var list = await _repository.GetAll();
            return list.Select(x => new NewsSourceDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }
    }
}