namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using System.Collections.Generic;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [Schema(nameof(GenericHost) + nameof(GenericHost.Test))]
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