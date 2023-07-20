namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseIndexViewQueryProvider : ISqlViewQueryProvider<DatabaseIndexColumn, Guid>,
                                                    IResolvable<ISqlViewQueryProvider<DatabaseIndexColumn, Guid>>,
                                                    ICollectionResolvable<ISqlViewQueryProvider>
    {
        private const string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseIndexColumn.PrimaryKey)}"",
ns.nspname  as ""{nameof(DatabaseIndexColumn.Schema)}"",
t.relname as ""{nameof(DatabaseIndexColumn.Table)}"",
i.relname as ""{nameof(DatabaseIndexColumn.Index)}"",
ix.indisunique as ""{nameof(DatabaseIndexColumn.Unique)}"",
case when ix.indpred is not null then trim(both ' ' from split_part(pg_get_indexdef(ix.indexrelid, 0, true), 'WHERE', 2)) end as ""{nameof(DatabaseIndexColumn.Predicate)}"",
a.attname as ""{nameof(DatabaseIndexColumn.Column)}"",
ix.idx < indnkeyatts as ""{nameof(DatabaseIndexColumn.IsKeyColumn)}"",
pg_get_indexdef(ix.indexrelid, 0, true) as ""{nameof(DatabaseIndexColumn.Definition)}""
from (select indrelid, indexrelid, unnest(indkey) as attnum, indnkeyatts, indisunique, indpred, generate_subscripts(indkey, 1) as idx from pg_index) ix
join pg_class t on t.oid = ix.indrelid
join pg_class i on i.oid = ix.indexrelid
join pg_namespace ns on ns.oid = i.relnamespace
join pg_attribute a on a.attrelid = t.oid and a.attnum = ix.attnum
where ns.nspname not in ('information_schema', 'public') and ns.nspname not like 'pg_%' and i.relname not like '%_pkey'";

        public string GetQuery()
        {
            return Query;
        }
    }
}