namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using System;
    using System.Linq;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class GenericArgumentsReceiverImpl : IGenericArgumentsReceiver
    {
        private readonly IVersioned<ITypeProvider> _typeProvider;

        public GenericArgumentsReceiverImpl(IVersioned<ITypeProvider> typeProvider)
        {
            _typeProvider = typeProvider;
        }

        /// <inheritdoc />
        public Type CloseByConstraints(Type type)
        {
            return AlreadyClosed(type)
                       ? type
                       : CloseByConstraintsInternal(type);
        }

        private Type CloseByConstraintsInternal(Type type)
        {
            var args = type.GetGenericArguments()
                           .Select(typeArgument => CloseByConstraints(ReceiveSatisfyingType(typeArgument)))
                           .ToArray();

            var closed = type.MakeGenericType(args);

            if (!AlreadyClosed(closed))
            {
                throw new ArgumentException($"Type {type.FullName} is not closed");
            }

            return closed;
        }

        private static bool AlreadyClosed(Type type)
        {
            return !type.IsGenericType
                   || type.IsConstructedGenericType;
        }

        private Type ReceiveSatisfyingType(Type typeArgument)
        {
            bool CheckTypeArgument(Type type) => type.FitsForTypeArgument(typeArgument);

            var typeProvider = _typeProvider.Current;

            var satisfyingType = typeProvider.OurTypes
                                             .FirstOrDefault(CheckTypeArgument)
                              ?? typeProvider.AllLoadedTypes
                                             .FirstOrDefault(CheckTypeArgument);

            return satisfyingType.EnsureNotNull($"Satisfying type for type argument {typeArgument} not found");
        }
    }
}