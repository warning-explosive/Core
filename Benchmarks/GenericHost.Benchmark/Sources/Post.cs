namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using System;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

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

        [ForeignKey(EnOnDeleteBehavior.Cascade)]
        public Blog Blog { get; set; }

        [ForeignKey(EnOnDeleteBehavior.Restrict)]
        public User User { get; set; }

        public DateTime DateTime { get; set; }

        public string Text { get; set; }
    }
}