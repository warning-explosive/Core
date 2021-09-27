namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System.Data;

    internal interface IAdvancedDbTransaction : IDbTransaction,
                                                ITrackableDbTransaction,
                                                IIdentifiedDbTransaction,
                                                IReadableDbTransaction,
                                                IWritableDbTransaction
    {
    }
}