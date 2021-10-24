namespace SpaceEngineers.Core.Modules.Test.DatabaseEntities.Relations
{
    using System;
    using DataAccess.Api.Model;

    internal record User : BaseDatabaseEntity<Guid>
    {
        public User(Guid primaryKey, string nickname)
            : base(primaryKey)
        {
            Nickname = nickname;
        }

        public string Nickname { get; private init; }
    }
}