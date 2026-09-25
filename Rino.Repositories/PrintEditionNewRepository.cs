using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface IPrintEditionNewRepository : IAsyncRepository<PrintEditionNew>
    { }

    public class PrintEditionNewRepository : BaseAsyncRepository<PrintEditionNew>, IPrintEditionNewRepository
    {
        public PrintEditionNewRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.PrintEditionNews;
        }
    }
}
