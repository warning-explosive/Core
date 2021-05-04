namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Data;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IDatabaseTransaction
    /// </summary>
    public interface IDatabaseTransaction : IResolvable
    {
        /// <summary>
        /// Are there any changes in the database transaction
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Opens database transaction
        /// </summary>
        /// <returns>Ongoing operation</returns>
        Task<IDbTransaction> Open();

        /// <summary>
        /// Closes database transaction
        /// </summary>
        /// <returns>Ongoing operation</returns>
        Task Close(bool commit);
    }
}