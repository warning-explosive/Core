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
    internal class DatabaseColumnViewQueryProvider : ISqlViewQueryProvider<DatabaseColumn, Guid>,
                                                     IResolvable<ISqlViewQueryProvider<DatabaseColumn, Guid>>,
                                                     ICollectionResolvable<ISqlViewQueryProvider>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseColumn.PrimaryKey)}"",
c.table_schema as ""{nameof(DatabaseColumn.Schema)}"",
c.table_name as ""{nameof(DatabaseColumn.Table)}"",
column_name as ""{nameof(DatabaseColumn.Column)}"",
ordinal_position as ""{nameof(DatabaseColumn.Position)}"",
data_type as ""{nameof(DatabaseColumn.DataType)}"",
case is_nullable when 'NO' then false when 'YES' then true end as ""{nameof(DatabaseColumn.Nullable)}"",
column_default as ""{nameof(DatabaseColumn.DefaultValue)}"",
numeric_scale as ""{nameof(DatabaseColumn.Scale)}"",
numeric_precision as ""{nameof(DatabaseColumn.Precision)}"",
character_maximum_length as ""{nameof(DatabaseColumn.Length)}""
from information_schema.columns c
join information_schema.tables t
on t.table_schema = c.table_schema and t.table_name = c.table_name  
where t.table_type != 'VIEW' and c.table_schema not in ('information_schema', 'public') and c.table_schema not like 'pg_%'
order by c.table_name, ordinal_position";

        public string GetQuery()
        {
            return Query;
        }
    }
}