﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseColumnConstraintViewQueryProvider : ISqlViewQueryProvider<DatabaseColumnConstraint, Guid>,
                                                               IResolvable<ISqlViewQueryProvider<DatabaseColumnConstraint, Guid>>,
                                                               ICollectionResolvable<ISqlViewQueryProvider>
    {
        private const string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseColumnConstraint.PrimaryKey)}"",
tc.table_schema as ""{nameof(DatabaseColumnConstraint.Schema)}"",
tc.table_name as ""{nameof(DatabaseColumnConstraint.Table)}"",
kcu.column_name as ""{nameof(DatabaseColumnConstraint.Column)}"",
case tc.constraint_type when 'PRIMARY KEY' then 'PrimaryKey' when 'FOREIGN KEY' then 'ForeignKey' end as ""{nameof(DatabaseColumnConstraint.ConstraintType)}"",
tc.constraint_name as ""{nameof(DatabaseColumnConstraint.ConstraintName)}"",
ccu.table_schema as ""{nameof(DatabaseColumnConstraint.ForeignSchema)}"",
ccu.table_name as ""{nameof(DatabaseColumnConstraint.ForeignTable)}"",
ccu.column_name as ""{nameof(DatabaseColumnConstraint.ForeignColumn)}""
from information_schema.table_constraints as tc
join information_schema.key_column_usage as kcu
on tc.constraint_name = kcu.constraint_name and tc.table_schema = kcu.table_schema
join information_schema.constraint_column_usage as ccu
on ccu.constraint_name = tc.constraint_name and ccu.table_schema = tc.table_schema
where tc.table_schema not in ('information_schema', 'public') and tc.table_schema not like 'pg_%'
order by tc.table_schema, tc.table_name, tc.constraint_type desc";

        public string GetQuery()
        {
            return Query;
        }
    }
}