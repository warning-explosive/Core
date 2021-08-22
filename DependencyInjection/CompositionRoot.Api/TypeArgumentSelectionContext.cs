namespace SpaceEngineers.Core.CompositionRoot.Api
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
        public TypeArgumentSelectionContext(Type openGeneric, Type typeArgument, IEnumerable<Type> matches)
        {
            OpenGeneric = openGeneric;
            TypeArgument = typeArgument;
            Matches = matches;
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
        public IEnumerable<Type> Matches { get; }
    }
}