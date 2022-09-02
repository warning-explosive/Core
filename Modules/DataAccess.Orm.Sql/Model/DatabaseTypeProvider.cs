namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseTypeProvider : IDatabaseTypeProvider,
                                          IResolvable<IDatabaseTypeProvider>
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
                    .Where(type => type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                                   && !type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>))
                                   && IsNotAbstraction(type))
                    .ToList();
            }

            static bool IsNotAbstraction(Type type)
            {
                return type != typeof(IUniqueIdentified)
                    && type != typeof(IUniqueIdentified<>)
                    && type != typeof(IDatabaseEntity)
                    && type != typeof(IDatabaseEntity<>)
                    && type != typeof(BaseDatabaseEntity<>)
                    && type != typeof(ISqlView<>)
                    && type != typeof(BaseSqlView<>)
                    && type != typeof(BaseMtmDatabaseEntity<,>);
            }
        }
    }
}