namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Verifiers;

    [Component(EnLifestyle.Singleton)]
    internal class SettingsConfigurationVerifier : IConfigurationVerifier,
                                                   ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;

        public SettingsConfigurationVerifier(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public void Verify()
        {
            var exceptions = new List<Exception>();

            var settingsTypes = _typeProvider
                .OurTypes
                .Where(type => typeof(ISettings).IsAssignableFrom(type) && type.IsConcreteType());

            VerifyPropertyInitializers(settingsTypes, exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void VerifyPropertyInitializers(
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            var properties = types
                .SelectMany(messageType => messageType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty))
                .Where(property => !property.IsEqualityContract())
                .Where(property => !property.HasInitializer() && property.SetIsAccessible());

            foreach (var property in properties)
            {
                exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable"));
            }
        }
    }
}