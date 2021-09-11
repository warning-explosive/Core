namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Builds database model from the existing database
    /// </summary>
    public interface IDatabaseModelBuilder : IModelBuilder, IResolvable
    {
    }
}