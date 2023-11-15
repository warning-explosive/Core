namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseEnumTypeViewQueryProvider : ISqlViewQueryProvider<DatabaseEnumType, Guid>,
                                                       IResolvable<ISqlViewQueryProvider<DatabaseEnumType, Guid>>,
                                                       ICollectionResolvable<ISqlViewQueryProvider>
    {
        private const string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseEnumType.PrimaryKey)}"",
nspname as ""{nameof(DatabaseEnumType.Schema)}"",
typname as ""{nameof(DatabaseEnumType.Type)}"",
enumlabel as ""{nameof(DatabaseEnumType.Value)}""
from pg_enum
join pg_type on pg_type.oid = enumtypid
join pg_namespace on pg_namespace.oid = pg_type.typnamespace";

        public string GetQuery()
        {
            return Query;
        }
    }
}