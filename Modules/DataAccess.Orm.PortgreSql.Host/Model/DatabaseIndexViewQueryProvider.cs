namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Host.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseIndexViewQueryProvider : ISqlViewQueryProvider<DatabaseIndex, Guid>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
    gen_random_uuid() as ""{nameof(DatabaseIndex.PrimaryKey)}"",
    schemaname as ""{nameof(DatabaseIndex.Schema)}"",
    tablename as ""{nameof(DatabaseIndex.Table)}"",
    indexname as ""{nameof(DatabaseIndex.Index)}"",
    indexdef as ""{nameof(DatabaseIndex.Definition)}"" 
FROM pg_indexes
where schemaname not in ('information_schema', 'public')
      and schemaname not like 'pg_%'
      and indexname not like '%_pkey'";

        public string GetQuery()
        {
            return Query;
        }
    }
}