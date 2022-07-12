namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
    [Index(nameof(Schema), nameof(View), nameof(Query), Unique = true)]
    internal class DatabaseView : BaseSqlView<Guid>
    {
        public DatabaseView(
            Guid primaryKey,
            string schema,
            string view,
            string query)
            : base(primaryKey)
        {
            Schema = schema;
            View = view;
            Query = query;
        }

        public string Schema { get; init; }

        public string View { get; init; }

        public string Query { get; init; }
    }
}