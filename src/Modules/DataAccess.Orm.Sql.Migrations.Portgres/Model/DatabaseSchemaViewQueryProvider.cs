namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseSchemaViewQueryProvider : ISqlViewQueryProvider<DatabaseSchema, Guid>,
                                                     IResolvable<ISqlViewQueryProvider<DatabaseSchema, Guid>>,
                                                     ICollectionResolvable<ISqlViewQueryProvider>
    {
        private const string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseSchema.PrimaryKey)}"",
schema_name as ""{nameof(DatabaseSchema.Name)}""
from information_schema.schemata
where schema_name not in ('information_schema', 'public') and schema_name not like 'pg_%'";

        public string GetQuery()
        {
            return Query;
        }
    }
}