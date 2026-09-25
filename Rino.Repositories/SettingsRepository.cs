using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Repositories
{
    public interface ISettingsRepository : IAsyncRepository<Settings>
    { }

    public class SettingsRepository : BaseAsyncRepository<Settings>, ISettingsRepository
    {
        public SettingsRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Settings;
        }
    }
}
