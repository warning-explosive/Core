namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using System;
    using System.Collections.Generic;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    [Schema(nameof(GenericHost) + nameof(Test))]
    internal record Participant : BaseDatabaseEntity<Guid>
    {
        public Participant(
            Guid primaryKey,
            string name,
            IReadOnlyCollection<Community> communities)
            : base(primaryKey)
        {
            Name = name;
            Communities = communities;
        }

        public string Name { get; set; }

        public IReadOnlyCollection<Community> Communities { get; set; }
    }
}