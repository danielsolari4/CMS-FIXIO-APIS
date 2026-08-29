using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
