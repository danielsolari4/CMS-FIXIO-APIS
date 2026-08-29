using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Core;
using Ray.Managers.MapperProfiles;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface IThemeManager : IManager<ThemeDto>
    {
        Task<ICollection<ThemeDto>> GetAll();
        Task<ThemeDto> Get();
    }

    public class ThemeManager : BaseManager, IThemeManager
    {
        private readonly IThemeRepository _repository;
        private readonly AppSettings _appSettings;

        public ThemeManager(IThemeRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ThemeProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ThemeDto> Add(ThemeDto entity)
        {
            if (entity == null)
                return null;

            var list = await _repository.GetAll();
            if (list.Any())
            {
                var theme = list.FirstOrDefault();
                if (theme == null) return entity;

                theme.Structure = entity.Structure;
                await _repository.Update(theme);
            }
            else
            {
                await _repository.Add(new Theme
                {
                    Structure = entity.Structure
                });
            }

            return entity;
        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }

        public async Task Delete(ThemeDto entity)
        {
            var dtoToDelete = await _repository.GetById(entity.Id);
            await _repository.Delete(dtoToDelete);
            await SolrHelper.DataImport(SolrCore.THEME, _appSettings.Solr);
        }

        public async Task<ICollection<ThemeDto>> GetAll()
        {
            try
            {
                var themes = new List<ThemeDto>();

                var all = await _repository.GetAll();

                foreach (var item in all)
                {
                    themes.Add(_Mapper.Map<ThemeDto>(item));

                }
                return themes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ThemeDto> Get()
        {
            var themes = await _repository.GetAll();
            return themes.Any() ? new ThemeDto { Structure = themes?.FirstOrDefault()?.Structure } : null;
        }

        public Task<ICollection<ThemeDto>> GetAll(int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }

        public async Task<ThemeDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var theme = await _repository.GetById(id);

            if (theme != null)
                return _Mapper.Map<ThemeDto>(theme);
            return null;
        }

        public async Task Update(ThemeDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var theme = (await _repository.Get(n => n.Id == dto.Id)).SingleOrDefault();

            if (theme == null)
                throw new ItemNotFoundException("Theme");

            await _repository.Update(theme);
            await SolrHelper.DataImport(SolrCore.THEME, _appSettings.Solr);
        }
    }
}
