namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IDataBaseModelBuilder
    /// </summary>
    public interface IDataBaseModelBuilder : IResolvable
    {
        /// <summary>
        /// Build model nodes from specified type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Built model nodes</returns>
        IEnumerable<ModelNode> BuildNodes(Type type);
    }
}