namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using System.Collections.Generic;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [Schema(nameof(GenericHost) + nameof(GenericHost.Test))]
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