namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
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

    [Component(EnLifestyle.Singleton)]
    internal class ModelChangeCommandBuilderComposite : IModelChangeCommandBuilderComposite,
                                                        IResolvable<IModelChangeCommandBuilderComposite>
    {
        private readonly IReadOnlyDictionary<Type, IModelChangeCommandBuilder> _map;

        public ModelChangeCommandBuilderComposite(IEnumerable<IModelChangeCommandBuilder> builders)
        {
            _map = builders.ToDictionary(static translator => translator.GetType().ExtractGenericArgumentAt(typeof(IModelChangeCommandBuilder<>)));
        }

        public Task<string> BuildCommand(IModelChange change, CancellationToken token)
        {
            return _map.TryGetValue(change.GetType(), out var builder)
                ? builder.BuildCommand(change, token)
                : throw new NotSupportedException($"Unsupported sql expression type {change.GetType()}");
        }
    }
}