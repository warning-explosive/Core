namespace SpaceEngineers.Core.DataAccess.Orm.Host.Model
{
    /// <summary>
    /// IModelValidator
    /// </summary>
    public interface IModelValidator
    {
        /// <summary>
        /// Validates database model
        /// </summary>
        /// <param name="model">Model</param>
        public void Validate(DatabaseNode model);
    }
}