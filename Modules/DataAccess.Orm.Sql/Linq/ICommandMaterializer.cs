namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System.Collections.Generic;
    using System.Threading;
    using Transaction;
    using Translation;

    internal interface ICommandMaterializerComposite : ICommandMaterializer
    {
    }

    internal interface ICommandMaterializer
    {
        IAsyncEnumerable<T> Materialize<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token);
    }

    internal interface ICommandMaterializer<TCommand>
        where TCommand : ICommand
    {
        IAsyncEnumerable<T> Materialize<T>(
            IAdvancedDatabaseTransaction transaction,
            TCommand command,
            CancellationToken token);
    }
}