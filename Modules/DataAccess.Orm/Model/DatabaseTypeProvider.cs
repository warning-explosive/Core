namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseTypeProvider : IDatabaseTypeProvider
    {
        private readonly ITypeProvider _typeProvider;

        private IReadOnlyCollection<Type>? _databaseEntities;

        public DatabaseTypeProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IEnumerable<Type> DatabaseEntities()
        {
            _databaseEntities ??= InitDatabaseEntities();
            return _databaseEntities;

            IReadOnlyCollection<Type> InitDatabaseEntities()
            {
                return _typeProvider
                    .OurTypes
                    .Where(type => type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>))
                                   && IsNotAbstraction(type))
                    .ToList();
            }

            static bool IsNotAbstraction(Type type)
            {
                return type != typeof(BaseDatabaseEntity<>)
                       && type != typeof(IDatabaseEntity<>);
            }
        }
    }
}