namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System;

    internal interface IIdentifiedDbTransaction
    {
        Guid Id { get; }

        DateTime Timestamp { get; }
    }
}