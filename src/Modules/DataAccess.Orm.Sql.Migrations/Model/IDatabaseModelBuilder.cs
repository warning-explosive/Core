namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System.Threading;
    using System.Threading.Tasks;
    using Transaction;

    /// <summary>
    /// Builds database model from the existing database
    /// </summary>
    public interface IDatabaseModelBuilder
    {
        /// <summary>
        /// Builds database model from the specified source
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Built model nodes</returns>
        Task<DatabaseNode?> BuildModel(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token);
    }
}