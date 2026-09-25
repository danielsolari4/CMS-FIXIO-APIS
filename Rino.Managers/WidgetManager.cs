using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Dtos.Mapping;
using Rino.Managers.Core;
using Rino.Repositories;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface IWidgetManager : IManager<WidgetDto>
    {
        Task<ICollection<WidgetTypeDto>> GetAllTypes();
    }

    public class WidgetManager : BaseManager, IWidgetManager
    {
        private readonly IWidgetRepository _repository;
        private readonly IWidgetTypeRepository _repositoryTypes;
        private readonly AppSettings _appSettings;

        public WidgetManager(IWidgetRepository repository, IWidgetTypeRepository repositoryTypes, AppSettings appSettings)
        {
            _repository = repository;
            _repositoryTypes = repositoryTypes;
            _appSettings = appSettings;
        }

        public async Task<ICollection<WidgetDto>> GetAll(int? skip = null, int? take = null)
        {
            var widgetList = new List<WidgetDto>();

            var widgetSet = (await _repository.GetAll());

            if (skip.HasValue)
                widgetSet = widgetSet.OrderBy(n => n.Id).Skip(skip.Value);

            if (take.HasValue)
                widgetSet = widgetSet.Take(take.Value);

            foreach (var widget in widgetSet)
                widgetList.Add(widget.Map());

            return widgetList;
        }

        public async Task<ICollection<WidgetTypeDto>> GetAllTypes()
        {
            var widgetList = await _repositoryTypes.GetAll();

            return widgetList.Select(x => new WidgetTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                Icon = x.Icon
            }).ToList();
        }

        public async Task<WidgetDto> GetById(int id)
        {
            var widget = await _repository.GetById(id);

            if (widget == null)
                throw new EntityException("dto");

            return widget.Map();
        }

        public async Task<WidgetDto> Add(WidgetDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            if ((await _repository.Get(x => x.Name == dto.Name && !x.IsDeleted)).Any())
                throw new AlreadyExistsException("widget");

            var entity = dto.Map();

            entity.CacheSolr = false;
            entity.IsDeleted = false;

            entity = await _repository.Add(entity);

            await SolrHelper.DataImport(SolrCore.WIDGET, _appSettings.Solr);

            return entity.Map();
        }

        public async Task Update(WidgetDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var widget = await _repository.GetById(dto.Id);

            if (widget == null)
                throw new ArgumentNullException(nameof(widget));

            if ((await _repository.Get(x => x.Name == dto.Name && !x.IsDeleted && x.Id != dto.Id)).Any())
                throw new AlreadyExistsException("widget");

            dto.CacheSolr = false;
            await _repository.Update(widget.Map(dto));

            await SolrHelper.DataImport(SolrCore.WIDGET, _appSettings.Solr);
        }

        public async Task Delete(WidgetDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var widget = await _repository.GetById(dto.Id);
            if (widget == null)
                throw new ArgumentNullException(nameof(widget));

            widget.IsDeleted = true;
            widget.CacheSolr = false;

            await _repository.Update(widget);

            await SolrHelper.DataImport(SolrCore.WIDGET, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }
    }
}
