namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Model;
    using Views;

    [Index(nameof(Schema), nameof(View), nameof(Query), Unique = true)]
    internal class DatabaseView : ISqlView<Guid>
    {
        public DatabaseView(Guid primaryKey, string schema, string view, string query)
        {
            PrimaryKey = primaryKey;
            Schema = schema;
            View = view;
            Query = query;
        }

        public Guid PrimaryKey { get; private set; }

        public string Schema { get; private set; }

        public string View { get; private set; }

        public string Query { get; private set; }
    }
}