using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
