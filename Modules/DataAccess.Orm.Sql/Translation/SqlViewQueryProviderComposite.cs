namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Model;

    [Component(EnLifestyle.Singleton)]
    internal class SqlViewQueryProviderComposite : ISqlViewQueryProviderComposite,
                                                   IResolvable<ISqlViewQueryProviderComposite>
    {
        private readonly IReadOnlyDictionary<Type, ISqlViewQueryProvider> _map;

        public SqlViewQueryProviderComposite(IEnumerable<ISqlViewQueryProvider> providers)
        {
            _map = providers.ToDictionary(provider => provider.GetType().ExtractGenericArgumentAt(typeof(ISqlViewQueryProvider<,>)));
        }

        public string GetQuery(Type type)
        {
            if (!type.IsSqlView())
            {
                throw new InvalidOperationException($"{type.FullName} should represent sql view");
            }

            return _map[type].GetQuery();
        }
    }
}