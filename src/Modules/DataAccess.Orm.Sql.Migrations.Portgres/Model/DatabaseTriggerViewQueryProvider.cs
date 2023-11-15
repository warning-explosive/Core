namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseTriggerViewQueryProvider : ISqlViewQueryProvider<DatabaseTrigger, Guid>,
                                                      IResolvable<ISqlViewQueryProvider<DatabaseTrigger, Guid>>,
                                                      ICollectionResolvable<ISqlViewQueryProvider>
    {
        private const string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseTrigger.PrimaryKey)}"",
ns.nspname as ""{nameof(DatabaseTrigger.Schema)}"",
trg.tgname as ""{nameof(DatabaseTrigger.Trigger)}"",
tbl.relname as ""{nameof(DatabaseTrigger.Table)}"",
p.proname as ""{nameof(DatabaseTrigger.Function)}"",
case trg.tgtype & cast(2 as int2)
    when 0 then 'after'::""{nameof(Migrations)}"".""{nameof(EnTriggerType)}""
    else 'before'::""{nameof(Migrations)}"".""{nameof(EnTriggerType)}""
end as ""{nameof(DatabaseTrigger.Type)}"",
case trg.tgtype & cast(28 as int2)
    when 16 then array['update'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}""]
    when  8 then array['delete'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}""]
    when  4 then array['insert'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}""]
    when 20 then array['insert'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}"", 'update'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}""]
    when 28 then array['insert'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}"", 'update'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}"", 'delete'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}""]
    when 24 then array['update'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}"", 'delete'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}""]
    when 12 then array['insert'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}"", 'delete'::""{nameof(Migrations)}"".""{nameof(EnTriggerEvent)}""]
end as ""{nameof(DatabaseTrigger.Event)}""
from pg_trigger trg
join pg_class tbl on trg.tgrelid = tbl.oid
join pg_proc p on trg.tgfoid = p.oid
join pg_namespace ns on tbl.relnamespace = ns.oid
where trg.tgname not like '%ConstraintTrigger%' and ns.nspname not in ('information_schema', 'public') and ns.nspname not like 'pg_%'";

        public string GetQuery()
        {
            return Query;
        }
    }
}