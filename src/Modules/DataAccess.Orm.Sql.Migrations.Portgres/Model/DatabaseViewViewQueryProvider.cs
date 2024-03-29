namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseViewViewQueryProvider : ISqlViewQueryProvider<DatabaseView, Guid>,
                                                   IResolvable<ISqlViewQueryProvider<DatabaseView, Guid>>,
                                                   ICollectionResolvable<ISqlViewQueryProvider>
    {
        private const string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
pgView.schema as ""{nameof(DatabaseView.Schema)}"",
pgView.viewName as ""{nameof(DatabaseView.View)}"",
sqlView.""{nameof(SqlView.Query)}"" as ""{nameof(DatabaseView.Query)}""
from (select viewname as viewName, schemaname as schema
      from pg_catalog.pg_views
      where schemaname not in ('information_schema', 'public') and schemaname not like 'pg_%'
      union all
      select matviewname as viewName, schemaname as schema
      from pg_catalog.pg_matviews
      where schemaname not in ('information_schema', 'public') and schemaname not like 'pg_%') pgView
join ""{nameof(Migrations)}"".""{nameof(SqlView)}"" sqlView
on pgView.schema = sqlView.""{nameof(SqlView.Schema)}"" and pgView.viewName = sqlView.""{nameof(SqlView.View)}""";

        public string GetQuery()
        {
            return Query;
        }
    }
}