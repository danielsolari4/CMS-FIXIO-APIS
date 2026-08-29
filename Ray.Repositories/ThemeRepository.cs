using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IThemeRepository : IAsyncRepository<Theme>
    { }

    public class ThemeRepository : BaseAsyncRepository<Theme>, IThemeRepository
    {
        public ThemeRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Themes;
        }
    }
}
