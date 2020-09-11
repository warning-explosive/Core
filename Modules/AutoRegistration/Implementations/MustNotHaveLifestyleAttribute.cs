namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Linq;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
    using Basics;

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