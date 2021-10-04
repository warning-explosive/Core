namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseIndexViewQueryProvider : ISqlViewQueryProvider<DatabaseIndex, Guid>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
    gen_random_uuid() as ""{nameof(DatabaseIndex.PrimaryKey)}"",
    schemaname as ""{nameof(DatabaseIndex.Schema)}"",
    tablename as ""{nameof(DatabaseIndex.Table)}"",
    indexname as ""{nameof(DatabaseIndex.Index)}""
FROM pg_indexes";

        public string GetQuery()
        {
            return Query;
        }
    }
}