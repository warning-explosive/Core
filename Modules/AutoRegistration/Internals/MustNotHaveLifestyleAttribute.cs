namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using SimpleInjector;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class MustNotHaveLifestyleAttribute : IConfigurationVerifier
    {
        private readonly ITypeProvider _typeProvider;

        public MustNotHaveLifestyleAttribute(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public void Verify()
        {
            _typeProvider
               .OurTypes
               .Where(type => (type.HasAttribute<UnregisteredAttribute>() || type.HasAttribute<ManualRegistrationAttribute>())
                           && type.HasAttribute<LifestyleAttribute>())
               .Each(type => throw new InvalidOperationException($"{type.FullName} marked with the {nameof(UnregisteredAttribute)} or {nameof(ManualRegistrationAttribute)} and mustn't be marked with the {nameof(LifestyleAttribute)}"));
        }
    }
}