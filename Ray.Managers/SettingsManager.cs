using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Dtos.Mapping;
using Ray.Managers.Core;
using Ray.Repositories;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface ISettingsManager : IManager<SettingsDto>
    {
    }

    public class SettingsManager : BaseManager, ISettingsManager
    {
        private readonly ISettingsRepository _repository;
        private readonly AppSettings _appSettings;
        public SettingsManager(ISettingsRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;
        }

        public async Task<ICollection<SettingsDto>> GetAll(int? skip = null, int? take = null)
        {
            var settingsList = new List<SettingsDto>();

            var settingsSet = (await _repository.GetAll());

            if (skip.HasValue)
                settingsSet = settingsSet.OrderBy(n => n.Id).Skip(skip.Value);

            if (take.HasValue)
                settingsSet = settingsSet.Take(take.Value);

            foreach (var settings in settingsSet)
                settingsList.Add(settings.Map());

            return settingsList;
        }

        public async Task<SettingsDto> GetById(int id)
        {
            var settings = await _repository.GetById(id);

            if (settings == null)
                throw new EntityException("dto");

            return settings.Map();
        }

        public async Task<SettingsDto> Add(SettingsDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new EntityException("Name");

            if (!(await _repository.Get(x => x.Name == dto.Name)).Any())
            {
                var entity = dto.Map();
                entity = await _repository.Add(entity);
                return entity.Map();
            }
            else
            {
                var exists = await _repository.Get(x => x.Name == dto.Name);
                var entity = exists.FirstOrDefault();
                entity.Value = dto.Value;
                await _repository.Update(entity);
            }

            await SolrHelper.DataImport(SolrCore.SETTINGSCORE, _appSettings.Solr, true);

            return dto;
        }

        public async Task Update(SettingsDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var settings = await _repository.GetById(dto.Id);

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if ((await _repository.Get(x => x.Name == dto.Name && x.Id != dto.Id)).Any())
                throw new AlreadyExistsException("settings");

            await _repository.Update(settings.Map(dto));

            await SolrHelper.DataImport(SolrCore.SETTINGSCORE, _appSettings.Solr, true);
        }

        public async Task Delete(SettingsDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var settings = await _repository.GetById(dto.Id);
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            await _repository.Update(settings);

            await SolrHelper.DataImport(SolrCore.SETTINGSCORE, _appSettings.Solr, true);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }
    }
}
