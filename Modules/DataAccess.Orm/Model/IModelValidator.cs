namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IModelValidator
    /// </summary>
    public interface IModelValidator : IResolvable
    {
        /// <summary>
        /// Validates database model
        /// </summary>
        /// <param name="model">Model</param>
        public void Validate(DatabaseNode model);
    }
}