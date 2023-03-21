namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
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