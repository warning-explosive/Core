namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using DataAccess.Api.Sql;
    using DataAccess.Api.Sql.Attributes;

    [Schema(nameof(GenericHost) + nameof(Test))]
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

        public Blog Blog { get; set; }

        public User User { get; set; }

        public DateTime DateTime { get; set; }

        public string Text { get; set; }
    }
}