namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Host.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseViewViewQueryProvider : ISqlViewQueryProvider<DatabaseView, Guid>,
                                                   IResolvable<ISqlViewQueryProvider<DatabaseView, Guid>>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
        gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
        viewname as ""{nameof(DatabaseView.View)}"",
        definition as ""{nameof(DatabaseView.Query)}"",
        schemaname as ""{nameof(DatabaseView.Schema)}""
    from pg_catalog.pg_views
    where schemaname not in ('information_schema', 'public')
          and schemaname not like 'pg_%'
    union all
    select
        gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
        matviewname as ""{nameof(DatabaseView.View)}"",
        definition as ""{nameof(DatabaseView.Query)}"",
        schemaname as ""{nameof(DatabaseView.Schema)}""
    from pg_catalog.pg_matviews
    where schemaname not in ('information_schema', 'public')
          and schemaname not like 'pg_%'";

        public string GetQuery()
        {
            return Query;
        }
    }
}