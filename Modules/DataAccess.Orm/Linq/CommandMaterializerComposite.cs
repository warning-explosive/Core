namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class CommandMaterializerComposite : ICommandMaterializerComposite,
                                                  IResolvable<ICommandMaterializerComposite>
    {
        private readonly IReadOnlyDictionary<Type, ICommandMaterializer> _map;

        public CommandMaterializerComposite(IEnumerable<ICommandMaterializer> materializers)
        {
            _map = materializers.ToDictionary(static translator => translator.GetType().ExtractGenericArgumentAt(typeof(ICommandMaterializer<>)));
        }

        public Task<object?> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            Type type,
            CancellationToken token)
        {
            return _map.TryGetValue(command.GetType(), out var materializer)
                ? materializer.MaterializeScalar(transaction, command, type, token)
                : throw new NotSupportedException($"Unsupported command type {command.GetType()}");
        }

        public IAsyncEnumerable<object?> Materialize(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            Type type,
            CancellationToken token)
        {
            return _map.TryGetValue(command.GetType(), out var materializer)
                ? materializer.Materialize(transaction, command, type, token)
                : throw new NotSupportedException($"Unsupported command type {command.GetType()}");
        }
    }
}