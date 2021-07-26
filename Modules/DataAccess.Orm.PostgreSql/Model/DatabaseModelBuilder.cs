namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Orm.Model.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder
    {
        public Task<DatabaseNode?> BuildModel()
        {
            // TODO: Build model tree from database (if exists)
            return Task.FromResult(default(DatabaseNode));
        }
    }
}