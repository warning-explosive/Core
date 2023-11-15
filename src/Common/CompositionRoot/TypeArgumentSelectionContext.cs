namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// GenericParameterSelectionContext
    /// </summary>
    public class TypeArgumentSelectionContext
    {
        /// <summary> .cctor </summary>
        /// <param name="openGeneric">Open generic type</param>
        /// <param name="typeArgument">Type argument</param>
        /// <param name="matches">Constraint matches</param>
        /// <param name="resolved">Resolved type arguments</param>
        public TypeArgumentSelectionContext(
            Type openGeneric,
            Type typeArgument,
            IReadOnlyCollection<Type> matches,
            IReadOnlyCollection<Type> resolved)
        {
            OpenGeneric = openGeneric;
            TypeArgument = typeArgument;
            Matches = matches;
            Resolved = resolved;
        }

        /// <summary>
        /// Open generic type
        /// </summary>
        public Type OpenGeneric { get; }

        /// <summary>
        /// Type argument
        /// </summary>
        public Type TypeArgument { get; }

        /// <summary>
        /// Constraint matches
        /// </summary>
        public IReadOnlyCollection<Type> Matches { get; }

        /// <summary>
        /// Resolved type arguments
        /// </summary>
        public IReadOnlyCollection<Type> Resolved { get; }
    }
}