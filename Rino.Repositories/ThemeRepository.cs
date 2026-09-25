using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
