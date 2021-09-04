namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.CompositionInfo
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// Dependency information about node in objects graph
    /// </summary>
    public interface IDependencyInfo
    {
        /// <summary>
        /// Service type
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// Implementation type
        /// </summary>
        Type ImplementationType { get; }

        /// <summary>
        /// Component lifestyle
        /// </summary>
        EnLifestyle? Lifestyle { get; }

        /// <summary>
        /// Component dependencies
        /// </summary>
        IReadOnlyCollection<IDependencyInfo> Dependencies { get; }

        /// <summary>
        /// Component depth in objects graph
        /// </summary>
        uint Depth { get; }

        /// <summary>
        /// Depth of the deepest child dependency
        /// </summary>
        uint ComplexityDepth { get; }

        /// <summary>
        /// IsCollectionResolvable attribute of service
        /// </summary>
        bool IsCollectionResolvable { get; }

        /// <summary>
        /// Cyclic dependency attribute
        /// </summary>
        bool IsCyclic { get; }

        /// <summary>
        /// IsUnregistered attribute
        /// </summary>
        bool IsUnregistered { get; }

        /// <summary>
        /// Parent
        /// </summary>
        public IDependencyInfo? Parent { get; }

        /// <summary>
        /// Execute action on IDependencyInfo and on his dependencies recursively
        /// </summary>
        /// <param name="action">action</param>
        void TraverseByGraph(Action<IDependencyInfo> action);

        /// <summary>
        /// Extract information from IDependencyInfo and from his dependencies recursively
        /// </summary>
        /// <param name="extractor">Extractor function</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Extracted information collection</returns>
        IEnumerable<TResult> ExtractFromGraph<TResult>(Func<IDependencyInfo, TResult> extractor);
    }
}