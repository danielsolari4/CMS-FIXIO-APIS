using System;
using Ray.Model.NewContext;

namespace Ray.Repositories.Core
{
    public interface IUnitOfWork : IDisposable
    {
        ModelContext Context { get; }
    }
}
