namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Model.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder
    {
        public DatabaseNode? BuildModel()
        {
            // TODO: Build model tree from database (if exists)
            return null;
        }
    }
}