namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    internal interface IInMemoryDatabase : IIdentifiedDatabase,
                                           IInitializableDatabase,
                                           ITransactionalDatabase,
                                           IReadableDatabase,
                                           IWritableDatabase
    {
    }
}