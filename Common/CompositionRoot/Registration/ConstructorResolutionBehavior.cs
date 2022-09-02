namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class ConstructorResolutionBehavior : IConstructorResolutionBehavior,
                                                   IResolvable<IConstructorResolutionBehavior>
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