using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IPrintEditionRepository : IAsyncRepository<PrintEdition>
    { }

    public class PrintEditionRepository : BaseAsyncRepository<PrintEdition>, IPrintEditionRepository
    {
        public PrintEditionRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.PrintEditions;
        }
    }
}
