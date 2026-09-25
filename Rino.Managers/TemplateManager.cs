using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rino.Dtos;
using Rino.Managers.Core;
using Rino.Managers.MapperProfiles;
using Rino.Repositories;

namespace Rino.Managers
{
    public interface ITemplateManager : IManager<TemplateDto>
    { }

    public class TemplateManager : BaseManager, ITemplateManager
    {
        private readonly ITemplateRepository _repository;

        public TemplateManager(ITemplateRepository repository)
        {
            _repository = repository;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<FullTemplateProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<TemplateDto>> GetAll(int? skip, int? take)
        {
            var template = new List<TemplateDto>();

            var templateSet = await _repository.GetAll();

            if (skip.HasValue)
                templateSet = templateSet.OrderBy(k => k.Id).Skip(skip.Value);

            if (take.HasValue)
                templateSet = templateSet.Take(take.Value);

            foreach (var itTemplate in templateSet)
            {
                template.Add(new TemplateDto
                {
                    Id =  itTemplate.Id,
                    Name = itTemplate.Name,
                    Class = itTemplate.Class
                });
            }

            return template;
        }


        public async Task<TemplateDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var template = await _repository.GetById(id);
            return template != null ? new TemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Class = template.Class
            } : null;
        }

        public async Task<TemplateDto> Add(TemplateDto dto)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public async Task Update(TemplateDto dto)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public Task Delete(TemplateDto entity)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

     
    }
}
