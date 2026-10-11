using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Dtos.Mapping;
using Rino.Managers.Core;
using Rino.Repositories;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface ISettingsManager : IManager<SettingsDto>
    {
    }

    public class SettingsManager : BaseManager, ISettingsManager
    {
        private readonly ISettingsRepository _repository;
        private readonly AppSettings _appSettings;
        private readonly ICacheInvalidationManager _cacheInvalidation;

        public SettingsManager(ISettingsRepository repository, AppSettings appSettings, ICacheInvalidationManager cacheInvalidation)
        {
            _repository = repository;
            _appSettings = appSettings;
            _cacheInvalidation = cacheInvalidation;
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
                dto = entity.Map();
            }
            else
            {
                var exists = await _repository.Get(x => x.Name == dto.Name);
                var entity = exists.FirstOrDefault();
                entity.Value = dto.Value;
                await _repository.Update(entity);
            }

            await ReindexAndInvalidate(dto.Name);

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

            await ReindexAndInvalidate(dto.Name);
        }

        public async Task Delete(SettingsDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var settings = await _repository.GetById(dto.Id);
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            await _repository.Update(settings);

            await ReindexAndInvalidate(settings.Name);
        }

        private async Task ReindexAndInvalidate(string settingName)
        {
            await SolrHelper.DataImportAndWait(SolrCore.SETTINGSCORE, _appSettings.Solr, true);

            string path = null;
            if (string.Equals(settingName, "Shorts", StringComparison.OrdinalIgnoreCase))
                path = "/api/media/shorts";
            else if (string.Equals(settingName, "LiveStream", StringComparison.OrdinalIgnoreCase))
                path = "/api/broadcast/live";
            else if (string.Equals(settingName, "AdsText", StringComparison.OrdinalIgnoreCase))
                path = "/ads.txt";

            if (path != null)
                _cacheInvalidation.InvalidatePaths(new[] { path }, $"settings:{settingName}");
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }
    }
}
