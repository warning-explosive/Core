namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Api.Sql;
    using DataAccess.Api.Sql.Attributes;

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