namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Api.Sql;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Host.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseViewViewQueryProvider : ISqlViewQueryProvider<DatabaseView, Guid>,
                                                   IResolvable<ISqlViewQueryProvider<DatabaseView, Guid>>,
                                                   ICollectionResolvable<ISqlViewQueryProvider>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
pgView.schema as ""{nameof(DatabaseView.Schema)}"",
pgView.viewName as ""{nameof(DatabaseView.View)}"",
sqlView.""Query"" as ""{nameof(DatabaseView.Query)}""
from (select viewname as viewName, schemaname as schema
      from pg_catalog.pg_views
      where schemaname not in ('information_schema', 'public') and schemaname not like 'pg_%'
      union all
      select matviewname as viewName, schemaname as schema
      from pg_catalog.pg_matviews
      where schemaname not in ('information_schema', 'public') and schemaname not like 'pg_%') pgView
join ""{nameof(Sql.Host.Migrations)}"".""{nameof(SqlView)}"" sqlView
on pgView.schema = sqlView.""Schema"" and pgView.viewName = sqlView.""View""";

        public string GetQuery()
        {
            return Query;
        }
    }
}