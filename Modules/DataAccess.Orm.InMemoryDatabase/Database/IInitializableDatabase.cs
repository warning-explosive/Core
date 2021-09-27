namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    internal interface IInitializableDatabase : IResolvable
    {
        Task CreateTable(Type table, CancellationToken token);
    }
}