namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using AutoRegistration.Api.Abstractions;

    internal interface IIdentifiedDatabase : IResolvable
    {
        string Name { get; }
    }
}