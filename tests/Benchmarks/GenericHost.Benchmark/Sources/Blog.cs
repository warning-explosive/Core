namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;

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