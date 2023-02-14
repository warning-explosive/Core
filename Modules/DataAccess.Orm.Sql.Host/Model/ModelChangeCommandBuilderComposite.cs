namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Linq;

    [Component(EnLifestyle.Singleton)]
    internal class ModelChangeCommandBuilderComposite : IModelChangeCommandBuilderComposite,
                                                        IResolvable<IModelChangeCommandBuilderComposite>
    {
        private readonly IReadOnlyDictionary<Type, IModelChangeCommandBuilder> _map;

        public ModelChangeCommandBuilderComposite(IEnumerable<IModelChangeCommandBuilder> builders)
        {
            _map = builders.ToDictionary(static translator => translator.GetType().ExtractGenericArgumentAt(typeof(IModelChangeCommandBuilder<>)));
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return _map.TryGetValue(change.GetType(), out var builder)
                ? builder.BuildCommands(change)
                : throw new NotSupportedException($"Detected model change: {change}. You should migrate database manually.");
        }
    }
}