using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface IURLRedirectRepository : IAsyncRepository<URLRedirect>
    { }

    public class URLRedirectRepository : BaseAsyncRepository<URLRedirect>, IURLRedirectRepository
    {
        public URLRedirectRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.URLRedirects;
        }
    }
}
