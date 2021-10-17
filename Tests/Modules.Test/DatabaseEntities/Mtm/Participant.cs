namespace SpaceEngineers.Core.Modules.Test.DatabaseEntities.Mtm
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Api.Model;

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

        public string Name { get; private init; }

        public IReadOnlyCollection<Community> Communities { get; private init; }
    }
}