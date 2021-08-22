namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Api.Abstractions;
    using Basics;

    /// <summary>
    /// DefaultConstructorResolutionBehavior
    /// </summary>
    public class DefaultConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        /// <inheritdoc />
        public bool TryGetConstructor(Type implementation, [NotNullWhen(true)] out ConstructorInfo? cctor)
        {
            if (!implementation.IsConcreteType())
            {
                cctor = null;
                return false;
            }

            var constructors = implementation.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            switch (constructors.Length)
            {
                case 0:
                    cctor = null;
                    return false;
                case > 1:
                    cctor = null;
                    return false;
                default:
                    cctor = constructors.Single();
                    return true;
            }
        }
    }
}