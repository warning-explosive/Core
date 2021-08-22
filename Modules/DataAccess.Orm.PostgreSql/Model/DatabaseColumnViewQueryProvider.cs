namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseColumnViewQueryProvider : IViewQueryProvider<DatabaseColumn>
    {
        private static readonly string Query = $@"select
    table_name as ""{nameof(DatabaseColumn.TableName)}"",
    column_name as ""{nameof(DatabaseColumn.ColumnName)}"",
	ordinal_position as {nameof(DatabaseColumn.Position)},
	data_type as ""{nameof(DatabaseColumn.DataType)}"",
	case is_nullable when 'NO' then false when 'YES' then true end as ""{nameof(DatabaseColumn.Nullable)}"",
	column_default as ""{nameof(DatabaseColumn.DefaultValue)}"",
	numeric_scale as ""{nameof(DatabaseColumn.Scale)}"",
	numeric_precision as ""{nameof(DatabaseColumn.Precision)}"",
	character_maximum_length as ""{nameof(DatabaseColumn.Length)}""
from information_schema.columns
where table_schema = '{{0}}'
order by table_name, ordinal_position;";

        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _databaseSettings;

        public DatabaseColumnViewQueryProvider(ISettingsProvider<PostgreSqlDatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public async Task<string> GetQuery(CancellationToken token)
        {
            var databaseSettings = await _databaseSettings
                .Get(token)
                .ConfigureAwait(false);

            return string.Format(Query, databaseSettings.Schema);
        }
    }
}