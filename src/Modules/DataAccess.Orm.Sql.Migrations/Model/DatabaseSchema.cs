namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    /// <summary>
    /// DatabaseSchema
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Name), Unique = true)]
    public record DatabaseSchema : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="name">name</param>
        public DatabaseSchema(Guid primaryKey, string name)
            : base(primaryKey)
        {
            Name = name;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }
}