namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using System;
    using System.Linq;
    using Basics;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class GenericArgumentsReceiverImpl : IGenericArgumentsReceiver
    {
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
                           .Select(typeArgument => CloseByConstraints(ReceiveType(typeArgument)))
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

        private static Type ReceiveType(Type typeArgument)
        {
            bool CheckTypeArgument(Type type) => type.FitsForTypeArgument(typeArgument);

            return TypeExtensions.OurTypes()
                                 .FirstOrDefault(CheckTypeArgument)
                   ?? TypeExtensions.AllLoadedTypes()
                                    .First(CheckTypeArgument);
        }
    }
}