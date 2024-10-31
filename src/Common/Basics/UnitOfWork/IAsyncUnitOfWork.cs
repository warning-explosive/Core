namespace SpaceEngineers.Core.Basics.UnitOfWork;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IAsyncUnitOfWork<TContext>
{
    Task ExecuteInTransaction(
        TContext context,
        Func<TContext, CancellationToken, Task> producer,
        bool saveChanges,
        CancellationToken token);
}