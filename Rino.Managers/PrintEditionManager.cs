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
    public interface IPrintEditionManager : IManager<PrintEditionDto>
    {

    }

    public class PrintEditionManager : BaseManager, IPrintEditionManager
    {
        private readonly IPrintEditionRepository _repository;
        private readonly IPrintEditionNewRepository _printEditionNewRepository;
        private readonly IAssetRepository _newsRepository;
        private readonly AppSettings _appSettings;

        public PrintEditionManager(IPrintEditionRepository repository, IPrintEditionNewRepository printEditionNewRepository, IAssetRepository newsRepository, AppSettings appSettings)
        {
            _repository = repository;
            _printEditionNewRepository = printEditionNewRepository;
            _newsRepository = newsRepository;
            _appSettings = appSettings;
        }

        public async Task<ICollection<PrintEditionDto>> GetAll(int? skip = null, int? take = null)
        {
            var PrintEditionList = new List<PrintEditionDto>();

            var PrintEditionSet = (await _repository.GetAll());

            if (skip.HasValue)
                PrintEditionSet = PrintEditionSet.OrderBy(n => n.Id).Skip(skip.Value);

            if (take.HasValue)
                PrintEditionSet = PrintEditionSet.Take(take.Value);

            foreach (var PrintEdition in PrintEditionSet)
                PrintEditionList.Add(PrintEdition.Map());

            return PrintEditionList;
        }

        public async Task<PrintEditionDto> GetById(int id)
        {
            var PrintEdition = await _repository.GetById(id);

            if (PrintEdition == null)
                throw new EntityException("dto");

            return PrintEdition.Map();
        }

        public async Task<PrintEditionDto> Add(PrintEditionDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            if ((await _repository.Get(x => (x.Identifier == dto.Identifier || x.Edition == dto.Edition) && x.NodeId == dto.NodeId)).Any())
                throw new AlreadyExistsException("PrintEdition");

            var entity = dto.Map();

            entity.CacheSolr = false;

            entity.LastModificationDate = DateTime.Now;
            entity.CreationDate = DateTime.Now;

            entity = await _repository.Add(entity);

            await SolrHelper.DataImport(SolrCore.PRINTEDITION, _appSettings.Solr);

            return entity.Map();
        }

        public async Task Update(PrintEditionDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var PrintEdition = await _repository.GetById(dto.Id);

            if (PrintEdition == null)
                throw new ArgumentNullException(nameof(PrintEdition));

            //if ((await _repository.Get(x => x.Identifier == dto.Identifier && x.Id != dto.Id)).Any())
            //    throw new AlreadyExistsException("PrintEdition");

            PrintEdition.CacheSolr = false;
            await _repository.Update(PrintEdition.Map(dto));

            await SolrHelper.DataImport(SolrCore.PRINTEDITION, _appSettings.Solr);
        }

        public async Task Delete(PrintEditionDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var PrintEdition = await _repository.GetById(dto.Id);
            if (PrintEdition == null)
                throw new ArgumentNullException(nameof(PrintEdition));

            foreach (var item in PrintEdition.PrintEditionNews.ToList())
            {
                item.New.CacheSolr = false;
                await _newsRepository.Update(item.New);
                await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);

                await _printEditionNewRepository.Delete(item);
            };

            await _repository.Delete(PrintEdition);
            await SolrHelper.DeleteDocumentById(SolrCore.PRINTEDITION, PrintEdition.Id, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }
    }
}
