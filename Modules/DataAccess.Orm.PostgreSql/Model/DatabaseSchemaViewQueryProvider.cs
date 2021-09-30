namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseSchemaViewQueryProvider : ISqlViewQueryProvider<DatabaseSchema, Guid>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
    gen_random_uuid() as ""{nameof(DatabaseSchema.PrimaryKey)}"",
    schema_name as ""{nameof(DatabaseSchema.Name)}""
from information_schema.schemata";

        public string GetQuery()
        {
            return Query;
        }
    }
}