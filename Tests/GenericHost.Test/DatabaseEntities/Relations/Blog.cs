namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities.Relations
{
    using System;
    using System.Collections.Generic;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [Schema(nameof(GenericHost) + nameof(GenericHost.Test))]
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