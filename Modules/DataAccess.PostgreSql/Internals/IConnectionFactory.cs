namespace SpaceEngineers.Core.DataAccess.PostgreSql.Internals
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using Npgsql;

    internal interface IConnectionFactory : IResolvable
    {
        Task<NpgsqlConnection> OpenConnection();
    }
}