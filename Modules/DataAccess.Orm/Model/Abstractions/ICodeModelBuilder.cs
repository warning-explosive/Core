namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Builds database model from the existing code base
    /// </summary>
    public interface ICodeModelBuilder : IModelBuilder, IResolvable
    {
    }
}