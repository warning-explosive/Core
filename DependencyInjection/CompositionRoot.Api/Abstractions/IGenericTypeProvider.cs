namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Close open or partially-closed generic types
    /// </summary>
    public interface IGenericTypeProvider : IResolvable
    {
        /// <summary>
        /// Receive all satisfying by type constraints types
        /// </summary>
        /// <param name="openGeneric">Open-generic type for closing</param>
        /// <param name="typeArgumentAt">Type-argument position</param>
        /// <returns>Satisfying types</returns>
        IEnumerable<Type> AllSatisfyingTypesAt(Type openGeneric, int typeArgumentAt = 0);

        /// <summary>
        /// Close open or partially-closed generic type by type constraints
        /// </summary>
        /// <param name="type">Generic type for closing</param>
        /// <param name="selector">Type selector</param>
        /// <returns>Closed generic type</returns>
        Type CloseByConstraints(Type type, Func<TypeArgumentSelectionContext, Type?>? selector = null);

        /// <summary>
        /// Default type argument selector
        /// - Order matches by IsGenericType ASC
        /// - SingleOrDefault
        /// </summary>
        /// <returns>Selector function</returns>
        Func<TypeArgumentSelectionContext, Type?> DefaultSelector();
    }
}