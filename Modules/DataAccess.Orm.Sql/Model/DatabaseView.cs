namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Views;

    internal class DatabaseView : ISqlView<Guid>
    {
        public DatabaseView(Guid primaryKey, string name, string query)
        {
            PrimaryKey = primaryKey;
            Name = name;
            Query = query;
        }

        public Guid PrimaryKey { get; private set; }

        public string Name { get; private set; }

        public string Query { get; private set; }
    }
}