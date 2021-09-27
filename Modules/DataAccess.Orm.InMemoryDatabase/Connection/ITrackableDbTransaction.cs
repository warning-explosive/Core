namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System;
    using System.Collections.Concurrent;
    using Database;

    internal interface ITrackableDbTransaction
    {
        ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> Changes { get; }
    }
}