using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Dtos.Mapping;
using Ray.Managers.Core;
using Ray.Repositories;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface ILayoutManager : IManager<LayoutDto>
    { }

    public class LayoutManager : BaseManager, ILayoutManager
    {
        private readonly ILayoutRepository _repository;
        private readonly AppSettings _appSettings;

        public LayoutManager(ILayoutRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;
        }

        public async Task<ICollection<LayoutDto>> GetAll(int? skip = null, int? take = null)
        {
            var layoutList = new List<LayoutDto>();

            var layoutSet = (await _repository.Get(n => !n.IsDeleted));

            if (skip.HasValue)
                layoutSet = layoutSet.OrderBy(n => n.Id).Skip(skip.Value);

            if (take.HasValue)
                layoutSet = layoutSet.Take(take.Value);

            foreach (var layout in layoutSet)
                layoutList.Add(layout.Map());

            return layoutList;
        }

        public async Task<LayoutDto> GetById(int id)
        {
            var layout = await _repository.GetById(id);

            if (layout == null)
                throw new EntityException("dto");

            return layout.Map();
        }

        public async Task<LayoutDto> Add(LayoutDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            if ((await _repository.Get(x => x.Name == dto.Name && !x.IsDeleted)).Any())
                throw new AlreadyExistsException("layout");

            var entity = dto.Map();

            entity.CacheSolr = false;
            entity.IsDeleted = false;

            entity = await _repository.Add(entity);


            //TODO:Crear nuevo core layout
            await SolrHelper.DataImport(SolrCore.LAYOUT, _appSettings.Solr);

            return entity.Map();
        }

        public async Task Update(LayoutDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var layout = await _repository.GetById(dto.Id);

            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            if ((await _repository.Get(x => x.Name == dto.Name && !x.IsDeleted && x.Id != dto.Id)).Any())
                throw new AlreadyExistsException("layout");

            layout.CacheSolr = false;
            await _repository.Update(layout.Map(dto));

            //TODO:Crear nuevo core layout
            await SolrHelper.DataImport(SolrCore.LAYOUT, _appSettings.Solr);
        }

        public async Task Delete(LayoutDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var layout = await _repository.GetById(dto.Id);
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            layout.IsDeleted = true;
            layout.CacheSolr = false;
            await _repository.Update(layout);

            await SolrHelper.DataImport(SolrCore.LAYOUT, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }
    }
}
