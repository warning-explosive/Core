namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class EventSourcingConfigurationVerifier : IConfigurationVerifier,
                                                        ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;

        public EventSourcingConfigurationVerifier(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public void Verify()
        {
            _typeProvider
               .OurTypes
               .Where(type => typeof(IAggregate).IsAssignableFrom(type)
                           && type.IsConcreteType())
               .SelectMany(VerifyAggregate)
               .Each(exception => throw exception.Rethrow());

            _typeProvider
               .OurTypes
               .Where(type => typeof(IDomainEvent).IsAssignableFrom(type)
                           && type.IsConcreteType())
               .SelectMany(VerifyDomainEvent)
               .Each(exception => throw exception.Rethrow());
        }

        private static IEnumerable<Exception> VerifyAggregate(Type type)
        {
            if (AggregateHasNoDefaultCctor(type, out var constructorException))
            {
                yield return constructorException;
            }

            foreach (var mutableAggregateException in AggregateHasPublicMutableProperties(type))
            {
                yield return mutableAggregateException;
            }
        }

        private static IEnumerable<Exception> VerifyDomainEvent(Type type)
        {
            if (DomainEventHasMultipleAggregates(type, out var constructorException))
            {
                yield return constructorException;
            }

            foreach (var initializerException in MissingPropertyInitializers(type))
            {
                yield return initializerException;
            }
        }

        private static bool AggregateHasNoDefaultCctor(Type type, [NotNullWhen(true)] out Exception? exception)
        {
            var parameterType = typeof(IEnumerable<>).MakeGenericType(typeof(IDomainEvent<>).MakeGenericType(type));

            var cctor = type
               .GetConstructors()
               .SingleOrDefault(info => info.IsPublic
                                        && info.GetParameters().Length == 1
                                        && info.GetParameters().Single().ParameterType == parameterType);

            if (cctor == null)
            {
                exception = new InvalidOperationException($"Type {type.FullName} should have one public constructor with required parameter {parameterType.FullName}");
                return true;
            }

            exception = null;
            return false;
        }

        private static IEnumerable<Exception> AggregateHasPublicMutableProperties(Type type)
        {
            var properties = type
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty)
               .Where(property => property.SetMethod != null && property.SetMethod.IsAccessible());

            foreach (var property in properties)
            {
                yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have private setter so as to grant aggregate immutability and behavior based design");
            }
        }

        private static bool DomainEventHasMultipleAggregates(Type type, [NotNullWhen(true)] out Exception? exception)
        {
            if (type.ExtractGenericArgumentsAt(typeof(IDomainEvent<>)).Count() > 1)
            {
                exception = new InvalidOperationException($"Domain event {type.FullName} shouldn't have multiple implementations of {typeof(IDomainEvent<>).FullName} abstraction");
                return true;
            }

            exception = null;
            return false;
        }

        private static IEnumerable<Exception> MissingPropertyInitializers(Type type)
        {
            var properties = type
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
               .Where(property => !property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase))
               .Where(property => !property.HasInitializer() || property.SetMethod.IsAccessible());

            foreach (var property in properties)
            {
                yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have private initializer (init modifier) so as to be deserializable");
            }
        }
    }
}