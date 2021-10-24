﻿namespace SpaceEngineers.Core.Modules.Test.DatabaseEntities.Relations
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Api.Model;

    internal record Blog : BaseDatabaseEntity<Guid>
    {
        public Blog(
            Guid primaryKey,
            string theme,
            IReadOnlyCollection<Post> posts)
            : base(primaryKey)
        {
            Theme = theme;
            Posts = posts;
        }

        public string Theme { get; private init; }

        public IReadOnlyCollection<Post> Posts { get; private init; }
    }
}