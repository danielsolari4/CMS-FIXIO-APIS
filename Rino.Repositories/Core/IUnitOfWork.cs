using System;
using Rino.Model.NewContext;

namespace Rino.Repositories.Core
{
    public interface IUnitOfWork : IDisposable
    {
        ModelContext Context { get; }
    }
}
