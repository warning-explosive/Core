namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// Builds database model from the existing database
    /// </summary>
    public interface IDatabaseModelBuilder : IModelBuilder, IResolvable
    {
    }
}