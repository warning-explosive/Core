namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;

    [Schema(nameof(GenericHost) + nameof(Test))]
    internal record Community : BaseDatabaseEntity<Guid>
    {
        public Community(
            Guid primaryKey,
            string name,
            IReadOnlyCollection<Participant> participants)
            : base(primaryKey)
        {
            Name = name;
            Participants = participants;
        }

        public string Name { get; set; }

        public IReadOnlyCollection<Participant> Participants { get; set; }
    }
}