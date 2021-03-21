namespace SpaceEngineers.Core.DataAccess.EntityFramework.Abstractions
{
    using AutoWiring.Api.Abstractions;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// IModelBuilder
    /// </summary>
    public interface IDatabaseModelBuilder : ICollectionResolvable<IDatabaseModelBuilder>
    {
        /// <summary>
        /// Build database model
        /// </summary>
        /// <param name="modelBuilder">ModelBuilder</param>
        void Build(ModelBuilder modelBuilder);
    }
}