namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Additional information about type
    /// </summary>
    public interface ITypeInfo
    {
        /// <summary>
        /// Original type
        /// </summary>
        Type OriginalType { get; }

        /// <summary>
        /// OriginalType dependencies from BeforeAttribute and AfterAttribute
        /// </summary>
        IReadOnlyCollection<Type> Dependencies { get; }

        /// <summary>
        /// OriginalType base types
        /// </summary>
        IReadOnlyCollection<Type> BaseTypes { get; }

        /// <summary>
        /// OriginalType GenericTypeDefinitions
        /// </summary>
        IReadOnlyCollection<Type> GenericTypeDefinitions { get; }

        /// <summary>
        /// OriginalType DeclaredInterfaces
        /// </summary>
        IReadOnlyCollection<Type> DeclaredInterfaces { get; }

        /// <summary>
        /// OriginalType GenericInterfaceDefinitions
        /// </summary>
        IReadOnlyCollection<Type> GenericInterfaceDefinitions { get; }

        /// <summary>
        /// Attributes
        /// </summary>
        IReadOnlyCollection<Attribute> Attributes { get; }
    }
}