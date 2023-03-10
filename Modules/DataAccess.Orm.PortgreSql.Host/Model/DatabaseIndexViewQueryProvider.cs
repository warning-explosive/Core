namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Host.Model;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseIndexViewQueryProvider : ISqlViewQueryProvider<DatabaseIndexColumn, Guid>,
                                                    IResolvable<ISqlViewQueryProvider<DatabaseIndexColumn, Guid>>,
                                                    ICollectionResolvable<ISqlViewQueryProvider>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseIndexColumn.PrimaryKey)}"",
ns.nspname  as ""{nameof(DatabaseIndexColumn.Schema)}"",
t.relname as ""{nameof(DatabaseIndexColumn.Table)}"",
i.relname as ""{nameof(DatabaseIndexColumn.Index)}"",
ix.indisunique as ""{nameof(DatabaseIndexColumn.Unique)}"",
a.attname as ""{nameof(DatabaseIndexColumn.Column)}"",
ix.idx < indnkeyatts as ""{nameof(DatabaseIndexColumn.IsKeyColumn)}"",
pg_get_indexdef(ix.indexrelid, 0, true) as ""{nameof(DatabaseIndexColumn.Definition)}""
from (select indrelid, indexrelid, unnest(indkey) as attnum, indnkeyatts, indisunique, generate_subscripts(indkey, 1) as idx from pg_index) ix
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