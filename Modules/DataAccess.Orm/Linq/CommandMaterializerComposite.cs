namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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

        public IAsyncEnumerable<T> Materialize<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return _map.TryGetValue(command.GetType(), out var materializer)
                ? materializer.Materialize<T>(transaction, command, token)
                : throw new NotSupportedException($"Unsupported command type {command.GetType()}");
        }
    }
}