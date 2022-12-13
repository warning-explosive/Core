namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Verifiers;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericDomain.Api.Abstractions;

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

            _typeProvider
               .OurTypes
               .Where(type => typeof(IEntity).IsAssignableFrom(type)
                           && type.IsConcreteType())
               .SelectMany(VerifyDomainEntity)
               .Each(exception => throw exception.Rethrow());

            _typeProvider
               .OurTypes
               .Where(type => typeof(IValueObject).IsAssignableFrom(type)
                           && type.IsConcreteType())
               .SelectMany(VerifyValueObject)
               .Each(exception => throw exception.Rethrow());
        }

        private static IEnumerable<Exception> VerifyAggregate(Type type)
        {
            if (IsNotImplementOpenGeneric(type, typeof(IAggregate<>), out var implementationException))
            {
                yield return implementationException;
            }

            if (AggregateHasMissingDefaultCctor(type, out var missingConstructorException))
            {
                yield return missingConstructorException;
            }

            foreach (var mutablePropertyException in AggregateHasPublicMutableProperties(type))
            {
                yield return mutablePropertyException;
            }
        }

        private static IEnumerable<Exception> VerifyDomainEvent(Type domainEvent)
        {
            if (TypeHasWrongModifier(domainEvent, out var modifierException))
            {
                yield return modifierException;
            }

            if (IsNotImplementOpenGeneric(domainEvent, typeof(IDomainEvent<>), out var implementationException))
            {
                yield return implementationException;
            }

            if (DomainEventHasMultipleAggregates(domainEvent, out var multipleAggregatesException))
            {
                yield return multipleAggregatesException;
            }

            var aggregate = domainEvent.ExtractGenericArgumentAt(typeof(IDomainEvent<>));

            if (AggregateHasNoDomainEvent(aggregate, domainEvent, out var aggregateHasNoDomainEventException))
            {
                yield return aggregateHasNoDomainEventException;
            }

            foreach (var initializerException in DomainObjectHasMissingPropertyInitializers(domainEvent))
            {
                yield return initializerException;
            }
        }

        private static IEnumerable<Exception> VerifyDomainEntity(Type domainEntity)
        {
            foreach (var initializerException in DomainObjectHasMissingPropertyInitializers(domainEntity))
            {
                yield return initializerException;
            }
        }

        private static IEnumerable<Exception> VerifyValueObject(Type valueObject)
        {
            foreach (var initializerException in DomainObjectHasMissingPropertyInitializers(valueObject))
            {
                yield return initializerException;
            }
        }

        private static bool TypeHasWrongModifier(
            Type type,
            [NotNullWhen(true)] out Exception? exception)
        {
            if (type.IsRecord())
            {
                exception = default;
                return false;
            }

            exception = new InvalidOperationException($"Type {type} should be defined as record");
            return true;
        }

        private static bool IsNotImplementOpenGeneric(
            Type type,
            Type openGeneric,
            [NotNullWhen(true)] out Exception? exception)
        {
            if (!type.IsSubclassOfOpenGeneric(openGeneric))
            {
                exception = new InvalidOperationException($"Type {type} should implement {openGeneric}");
                return true;
            }

            exception = null;
            return false;
        }

        private static bool AggregateHasMissingDefaultCctor(
            Type aggregate,
            [NotNullWhen(true)] out Exception? exception)
        {
            var parameterType = typeof(Array)
               .CallMethod(nameof(Array.Empty))
               .WithTypeArgument(typeof(IDomainEvent<>).MakeGenericType(aggregate))
               .Invoke()
               .GetType();

            var cctor = aggregate
               .GetConstructors()
               .SingleOrDefault(info => info.IsPublic
                                        && info.GetParameters().Length == 1
                                        && info.GetParameters().Single().ParameterType == parameterType);

            if (cctor == null)
            {
                exception = new InvalidOperationException($"Type {aggregate.FullName} should have one public constructor with required parameter {parameterType.FullName}");
                return true;
            }

            exception = null;
            return false;
        }

        private static IEnumerable<Exception> AggregateHasPublicMutableProperties(Type aggregate)
        {
            var properties = aggregate
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty)
               .Where(property => property.SetMethod != null && property.SetIsAccessible());

            foreach (var property in properties)
            {
                yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have no setter or have private one so as to grant aggregate immutability and behavior based design");
            }
        }

        private static bool DomainEventHasMultipleAggregates(
            Type domainEvent,
            [NotNullWhen(true)] out Exception? exception)
        {
            if (domainEvent.ExtractGenericArgumentsAt(typeof(IDomainEvent<>)).Count() > 1)
            {
                exception = new InvalidOperationException($"Domain event {domainEvent.FullName} shouldn't have multiple implementations of {typeof(IDomainEvent<>).FullName} abstraction");
                return true;
            }

            exception = null;
            return false;
        }

        private static IEnumerable<Exception> DomainObjectHasMissingPropertyInitializers(Type domainEvent)
        {
            var properties = domainEvent
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
               .Where(property => !property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase))
               .Where(property => !(property.HasInitializer() && property.SetIsAccessible()));

            foreach (var property in properties)
            {
                yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable");
            }
        }

        private static bool AggregateHasNoDomainEvent(
            Type aggregate,
            Type domainEvent,
            [NotNullWhen(true)] out Exception? exception)
        {
            var hasDomainEvent = typeof(IHasDomainEvent<,>).MakeGenericType(aggregate, domainEvent);

            if (!hasDomainEvent.IsAssignableFrom(aggregate))
            {
                exception = new InvalidOperationException($"Type {aggregate} should implement {hasDomainEvent}");
                return true;
            }

            exception = null;
            return false;
        }
    }
}