namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Model;
    using Views;

    [Index(nameof(Name), Unique = true)]
    internal class DatabaseSchema : ISqlView<Guid>
    {
        public DatabaseSchema(Guid primaryKey, string name)
        {
            PrimaryKey = primaryKey;
            Name = name;
        }

        public Guid PrimaryKey { get; }

        public string Name { get; }
    }
}