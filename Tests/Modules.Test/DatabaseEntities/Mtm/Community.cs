﻿namespace SpaceEngineers.Core.Modules.Test.DatabaseEntities.Mtm
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Api.Model;

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

        public string Name { get; private init; }

        public IReadOnlyCollection<Participant> Participants { get; private init; }
    }
}