using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
