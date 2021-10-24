namespace SpaceEngineers.Core.Modules.Test.DatabaseEntities.Relations
{
    using System;
    using DataAccess.Api.Model;

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