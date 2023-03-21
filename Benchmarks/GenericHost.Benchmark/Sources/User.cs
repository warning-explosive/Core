namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using System;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    [Schema(nameof(GenericHost) + nameof(Test))]
    internal record User : BaseDatabaseEntity<Guid>
    {
        public User(Guid primaryKey, string nickname)
            : base(primaryKey)
        {
            Nickname = nickname;
        }

        public string Nickname { get; set; }
    }
}