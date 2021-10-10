namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseViewViewQueryProvider : ISqlViewQueryProvider<DatabaseView, Guid>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select * 
from (select
        gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
        viewname as ""{nameof(DatabaseView.View)}"",
        definition as ""{nameof(DatabaseView.Query)}"",
        schemaname as ""{nameof(DatabaseView.Schema)}""
    from pg_catalog.pg_views
    union all
    select
        gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
        matviewname as ""{nameof(DatabaseView.View)}"",
        definition as ""{nameof(DatabaseView.Query)}"",
        schemaname as ""{nameof(DatabaseView.Schema)}""
    from pg_catalog.pg_matviews) views_union
where views_union.""{nameof(DatabaseView.Schema)}"" not in ('information_schema', 'public')
      and views_union.""{nameof(DatabaseView.Schema)}"" not like 'pg_%'";

        public string GetQuery()
        {
            return Query;
        }
    }
}