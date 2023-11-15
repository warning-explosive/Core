namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
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