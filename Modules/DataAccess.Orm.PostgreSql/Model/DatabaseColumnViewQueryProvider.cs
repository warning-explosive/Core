namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseColumnViewQueryProvider : ISqlViewQueryProvider<DatabaseColumn, Guid>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
    gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
    table_schema as ""{nameof(DatabaseColumn.Schema)}"",
    table_name as ""{nameof(DatabaseColumn.Table)}"",
    column_name as ""{nameof(DatabaseColumn.Column)}"",
	ordinal_position as {nameof(DatabaseColumn.Position)},
	data_type as ""{nameof(DatabaseColumn.DataType)}"",
	case is_nullable when 'NO' then false when 'YES' then true end as ""{nameof(DatabaseColumn.Nullable)}"",
	column_default as ""{nameof(DatabaseColumn.DefaultValue)}"",
	numeric_scale as ""{nameof(DatabaseColumn.Scale)}"",
	numeric_precision as ""{nameof(DatabaseColumn.Precision)}"",
	character_maximum_length as ""{nameof(DatabaseColumn.Length)}""
from information_schema.columns
where table_schema not in ('information_schema', 'public')
      and table_schema not like 'pg_%'
order by table_name, ordinal_position";

        public string GetQuery()
        {
            return Query;
        }
    }
}