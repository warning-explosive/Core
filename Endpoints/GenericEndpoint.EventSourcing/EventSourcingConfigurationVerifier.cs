namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing
{
    using System;
    using System.Collections.Generic;
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
            var exceptions = new List<Exception>();

            var aggregates = _typeProvider
                .OurTypes
                .Where(type => typeof(IAggregate).IsAssignableFrom(type) && type.IsConcreteType());

            VerifyAggregates(aggregates, exceptions);

            var domainEvents = _typeProvider
               .OurTypes
               .Where(type => typeof(IDomainEvent).IsAssignableFrom(type) && type.IsConcreteType());

            VerifyDomainEvents(domainEvents, exceptions);

            var domainEntities = _typeProvider
               .OurTypes
               .Where(type => typeof(IEntity).IsAssignableFrom(type) && type.IsConcreteType());

            VerifyDomainEntities(domainEntities, exceptions);

            var valueObjects = _typeProvider
               .OurTypes
               .Where(type => typeof(IValueObject).IsAssignableFrom(type) && type.IsConcreteType());

            VerifyValueObjects(valueObjects, exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void VerifyAggregates(
            IEnumerable<Type> aggregates,
            ICollection<Exception> exceptions)
        {
            foreach (var aggregate in aggregates)
            {
                if (!aggregate.IsSubclassOfOpenGeneric(typeof(IAggregate<>)))
                {
                    exceptions.Add(new InvalidOperationException($"Type {aggregate.FullName} should implement {typeof(IAggregate<>).FullName}"));
                }

                VerifyConstructors(aggregate, exceptions);
                VerifyPublicMutableProperties(aggregate, exceptions);
            }

            static void VerifyConstructors(
                Type aggregate,
                ICollection<Exception> exceptions)
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
                    exceptions.Add(new InvalidOperationException($"Type {aggregate.FullName} should have one public constructor with required parameter {parameterType.FullName}"));
                }
            }

            static void VerifyPublicMutableProperties(
                Type aggregate,
                ICollection<Exception> exceptions)
            {
                var properties = aggregate
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Where(property => property.SetMethod != null && property.SetIsAccessible());

                foreach (var property in properties)
                {
                    exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have no setter or have private one so as to grant aggregate immutability and behavior based design"));
                }
            }
        }

        private static void VerifyDomainEvents(
            IEnumerable<Type> domainEvents,
            ICollection<Exception> exceptions)
        {
            foreach (var domainEvent in domainEvents)
            {
                if (!domainEvent.IsSubclassOfOpenGeneric(typeof(IDomainEvent<>)))
                {
                    exceptions.Add(new InvalidOperationException($"Type {domainEvent.FullName} should implement {typeof(IDomainEvent<>).FullName}"));
                }

                VerifyWrongModifiers(domainEvent, exceptions);
                VerifyMissingPropertyInitializers(domainEvent, exceptions);
                VerifyMultipleAggregates(domainEvent, exceptions);
                VerifyAggregateInterfaces(domainEvent, exceptions);
            }

            static void VerifyWrongModifiers(
                Type type,
                ICollection<Exception> exceptions)
            {
                if (!type.IsRecord())
                {
                    exceptions.Add(new InvalidOperationException($"Type {type} should be defined as record"));
                }
            }

            static void VerifyMultipleAggregates(
                Type domainEvent,
                ICollection<Exception> exceptions)
            {
                if (domainEvent.ExtractGenericArgumentsAt(typeof(IDomainEvent<>)).Count() > 1)
                {
                    exceptions.Add(new InvalidOperationException($"Domain event {domainEvent.FullName} shouldn't have multiple implementations of {typeof(IDomainEvent<>).FullName} abstraction"));
                }
            }

            static void VerifyAggregateInterfaces(
                Type domainEvent,
                ICollection<Exception> exceptions)
            {
                var aggregate = domainEvent.ExtractGenericArgumentAt(typeof(IDomainEvent<>));

                var hasDomainEvent = typeof(IHasDomainEvent<,>).MakeGenericType(aggregate, domainEvent);

                if (!hasDomainEvent.IsAssignableFrom(aggregate))
                {
                    exceptions.Add(new InvalidOperationException($"Type {aggregate} should implement {hasDomainEvent}"));
                }
            }
        }

        private static void VerifyDomainEntities(
            IEnumerable<Type> domainEntities,
            ICollection<Exception> exceptions)
        {
            foreach (var domainEntity in domainEntities)
            {
                VerifyMissingPropertyInitializers(domainEntity, exceptions);
            }
        }

        private static void VerifyValueObjects(
            IEnumerable<Type> valueObjects,
            ICollection<Exception> exceptions)
        {
            foreach (var valueObject in valueObjects)
            {
                VerifyMissingPropertyInitializers(valueObject, exceptions);
            }
        }

        private static void VerifyMissingPropertyInitializers(
            Type type,
            ICollection<Exception> exceptions)
        {
            var properties = type
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
               .Where(property => !property.IsEqualityContract())
               .Where(property => !(property.HasInitializer() && property.SetIsAccessible()));

            foreach (var property in properties)
            {
                exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable"));
            }
        }
    }
}