namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Type info storage abstraction
    /// </summary>
    public interface ITypeProvider : IResolvable
    {
        /// <summary>
        /// All assemblies loaded in AppDomain
        /// </summary>
        IReadOnlyCollection<Assembly> AllLoadedAssemblies { get; }

        /// <summary>
        /// All types loaded in AppDomain
        /// </summary>
        IReadOnlyCollection<Type> AllLoadedTypes { get; }

        /// <summary>
        /// All our assemblies loaded in AppDomain
        /// </summary>
        IReadOnlyCollection<Assembly> OurAssemblies { get; }

        /// <summary>
        /// All our types loaded in AppDomain
        /// </summary>
        IReadOnlyCollection<Type> OurTypes { get; }

        /// <summary>
        /// Type cache -> assembly name -> type full name -> type
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> TypeCache { get; }

        /// <summary>
        /// Does type located in our assembly
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Result of check</returns>
        bool IsOurType(Type type);
    }
}