namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;
    using Views;

    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
    [Index(nameof(Name), Unique = true)]
    internal class DatabaseSchema : BaseSqlView<Guid>
    {
        public DatabaseSchema(Guid primaryKey, string name)
            : base(primaryKey)
        {
            Name = name;
        }

        public string Name { get; private init; }
    }
}