namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Api.Sql;
    using DataAccess.Api.Sql.Attributes;

    [Schema(nameof(GenericHost) + nameof(Test))]
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

        public string Theme { get; set; }

        public IReadOnlyCollection<Post> Posts { get; set; }
    }
}