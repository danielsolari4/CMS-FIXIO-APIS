using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers.Core;
using Rino.Repositories;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface IPrintEditionNewManager : IManager<PrintEditionNewDto>
    {
        
    }

    public class PrintEditionNewManager : BaseManager, IPrintEditionNewManager
    {
        private readonly IPrintEditionNewRepository _repository;
        private readonly AppSettings _appSettings;

        public PrintEditionNewManager(IPrintEditionNewRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;
        }

        public async Task<ICollection<PrintEditionNewDto>> GetAll(int? skip = null, int? take = null)
        {
           throw new NotImplementedException();
        }

        public async Task<PrintEditionNewDto> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PrintEditionNewDto> Add(PrintEditionNewDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task Update(PrintEditionNewDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(PrintEditionNewDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var PrintEditionNew = await _repository.GetById(dto.Id);
            if (PrintEditionNew == null)
                throw new ArgumentNullException(nameof(PrintEditionNew));

            await _repository.Delete(PrintEditionNew);

            await SolrHelper.DataImport(SolrCore.PRINTEDITION, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }
    }
}
