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
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface IKeywordManager : IManager<KeywordDto>
    {
        Task SetFeaturedKeywords(SetFeaturedKeywordsDtoBindingModel dto);
    }

    public class KeywordManager : BaseManager, IKeywordManager
    {
        private readonly IKeywordRepository _repository;
        private readonly AppSettings _appSettings;

        public KeywordManager(IKeywordRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<KeywordProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<KeywordDto>> GetAll(int? skip, int? take)
        {
            var keyword = new List<KeywordDto>();

            var keywordSet = await _repository.GetAll();

            if (skip.HasValue)
                keywordSet = keywordSet.OrderBy(k => k.Id).Skip(skip.Value);

            if (take.HasValue)
                keywordSet = keywordSet.Take(take.Value);

            foreach (var kw in keywordSet)
                keyword.Add(MapToDto(kw));

            return keyword;
        }

        public async Task<KeywordDto> GetById(int id)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public async Task<KeywordDto> Add(KeywordDto dto)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public async Task Update(KeywordDto dto)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public async Task SetFeaturedKeywords(SetFeaturedKeywordsDtoBindingModel dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.KeywordIds))
            {
                var keywords = dto.KeywordIds.Split(',').Select(int.Parse).ToList();

                if (keywords.Any())
                {
                    var featuredKeywords = (await _repository.GetAll()).Where(k => k.Featured);

                    var keywordsToRemove = featuredKeywords.Where(keyword => !keywords.Any(m => m == keyword.Id)).ToArray();

                    if (keywordsToRemove != null && keywordsToRemove.Any())
                        foreach (var keyword in keywordsToRemove)
                        {
                            keyword.Featured = false;
                            keyword.LastModificationDate = DateTime.UtcNow;
                            keyword.CacheSolr = false;
                            await _repository.Update(keyword);
                        }

                    var keywordsToAdd = keywords.Where(kwId => !featuredKeywords.Any(kw => kw.Id == kwId));

                    if (keywordsToAdd != null && keywordsToAdd.Any())
                        foreach (var id in keywordsToAdd)
                        {
                            var keyword = await _repository.GetById(id);

                            if (keyword != null && keyword.IsEnabled)
                            {
                                keyword.Featured = true;
                                keyword.LastModificationDate = DateTime.UtcNow;
                                keyword.CacheSolr = false;
                                await _repository.Update(keyword);
                            }
                        }
                }

                await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
            }
        }

        public Task Delete(KeywordDto entity)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private KeywordDto MapToDto(Keyword keyword)
        {
            var dto = _Mapper.Map<KeywordDto>(keyword);
            return dto;
        }
    }
}
