﻿namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [Schema(nameof(GenericHost) + nameof(GenericHost.Test))]
    internal record Post : BaseDatabaseEntity<Guid>
    {
        public Post(
            Guid primaryKey,
            Blog blog,
            User user,
            DateTime dateTime,
            string text)
            : base(primaryKey)
        {
            Blog = blog;
            User = user;
            DateTime = dateTime;
            Text = text;
        }

        public Blog Blog { get; private init; }

        public User User { get; private init; }

        public DateTime DateTime { get; private init; }

        public string Text { get; private init; }
    }
}