using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
