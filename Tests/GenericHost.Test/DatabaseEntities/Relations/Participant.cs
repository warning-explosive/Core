﻿namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Api.Sql;
    using DataAccess.Api.Sql.Attributes;

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