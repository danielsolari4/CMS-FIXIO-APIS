using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface ITemplateRepository : IAsyncRepository<Template>
    { }

    public class TemplateRepository : BaseAsyncRepository<Template>, ITemplateRepository
    {
        public TemplateRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Templates;
        }
    }
}
