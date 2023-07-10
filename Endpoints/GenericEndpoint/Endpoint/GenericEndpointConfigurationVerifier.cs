namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Extensions;
    using CompositionRoot.Registration;
    using CompositionRoot.Verifiers;
    using Contract;
    using Contract.Abstractions;
    using Contract.Attributes;
    using Messaging;
    using Messaging.MessageHeaders;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier,
                                                          ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly ITypeProvider _typeProvider;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly IRegistrationsContainer _registrations;

        public GenericEndpointConfigurationVerifier(
            EndpointIdentity endpointIdentity,
            ITypeProvider typeProvider,
            IIntegrationTypeProvider integrationTypeProvider,
            IRegistrationsContainer registrations)
        {
            _endpointIdentity = endpointIdentity;
            _typeProvider = typeProvider;
            _integrationTypeProvider = integrationTypeProvider;
            _registrations = registrations;
        }

        /// <inheritdoc />
        public void Verify()
        {
            var exceptions = new List<Exception>();

            VerifyEndpointLogicalName(_endpointIdentity, exceptions);

            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EndpointCommands(), exceptions);
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EventsSubscriptions(), exceptions);
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EndpointRequests(), exceptions);
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.RepliesSubscriptions(), exceptions);

            var messages = _integrationTypeProvider.IntegrationMessageTypes();

            VerifyModifiers(messages, exceptions);
            VerifyConstructors(messages, exceptions);
            VerifyOwnedAttribute(messages, exceptions);
            VerifyMessageInterfaces(messages, exceptions);
            VerifyPropertyInitializers(messages, exceptions);
            VerifyTypeArguments(messages, exceptions);

            var headers = _typeProvider
                .OurTypes
                .Where(typeof(IIntegrationMessageHeader).IsAssignableFrom)
                .ToList();

            VerifyModifiers(headers, exceptions);
            VerifyConstructors(headers, exceptions);
            VerifyPropertyInitializers(headers, exceptions);
            VerifyTypeArguments(headers, exceptions);

            VerifyHandlerExistence(_integrationTypeProvider.EndpointCommands(), exceptions);
            VerifyHandlerExistence(_integrationTypeProvider.EndpointRequests(), exceptions);

            VerifyMessageHandlersLifestyle(exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void VerifyEndpointLogicalName(
            EndpointIdentity endpointIdentity,
            ICollection<Exception> exceptions)
        {
            var pattern = new Regex("[^a-zA-Z\\d]", RegexOptions.Compiled);

            if (pattern.IsMatch(endpointIdentity.LogicalName))
            {
                exceptions.Add(new InvalidOperationException($"Endpoint {endpointIdentity} logical name should contain only letters and digits"));
            }
        }

        private static void VerifyMessageNames(
            EndpointIdentity endpointIdentity,
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            types = types.Where(type => HasWrongName(endpointIdentity, type));

            foreach (var type in types)
            {
                exceptions.Add(new InvalidOperationException($"Message name {type.FullName} should be less or equal than 255 bytes"));
            }

            static bool HasWrongName(EndpointIdentity endpointIdentity, Type type)
            {
                var left = type.GenericTypeDefinitionOrSelf().FullName!;
                var right = endpointIdentity.LogicalName;

                return left.Length + right.Length + 1 > 255;
            }
        }

        private static void VerifyMessageInterfaces(
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            types = types.Where(ImplementsSeveralSpecializedInterfaces().Not());

            foreach (var type in types)
            {
                exceptions.Add(new InvalidOperationException($"Message {type.FullName} must implement only one specialized interface (command, request, event or just message)"));
            }

            static Func<Type, bool> ImplementsSeveralSpecializedInterfaces()
            {
                return type =>
                {
                    var sum = type.IsCommand().Bit()
                              + type.IsEvent().Bit()
                              + type.IsRequest().Bit()
                              + type.IsReply().Bit();

                    return sum == 1;
                };
            }
        }

        private static void VerifyOwnedAttribute(
            IReadOnlyCollection<Type> types,
            ICollection<Exception> exceptions)
        {
            foreach (var type in types)
            {
                if (type.IsMessageContractAbstraction())
                {
                    continue;
                }

                if (type.IsReply()
                    && !types.Any(type.IsReplyOnRequest))
                {
                    exceptions.Add(new InvalidOperationException($"Reply {type.FullName} should have at least one corresponding {nameof(IIntegrationRequest<IIntegrationReply>)}"));
                }

                if (type.IsReply()
                    && type.HasAttribute<OwnedByAttribute>())
                {
                    exceptions.Add(new InvalidOperationException($"Reply {type.FullName} should not be marked by {nameof(OwnedByAttribute)}"));
                }

                if (!type.IsReply()
                    && !type.HasAttribute<OwnedByAttribute>())
                {
                    exceptions.Add(new InvalidOperationException($"Message {type.FullName} should be marked by {nameof(OwnedByAttribute)} in order to provide automatic service discovery"));
                }
            }
        }

        private static void VerifyConstructors(
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            types = types
                .Where(type => type.IsConcreteType())
                .Where(HasMissingDefaultCctor);

            foreach (var type in types)
            {
                exceptions.Add(new InvalidOperationException($"Type {type.FullName} should have default constructor (optionally obsolete) so as to be deserialized by System.Text.Json"));
            }

            static bool HasMissingDefaultCctor(Type type)
            {
                return type
                    .GetConstructors()
                    .All(cctor => cctor.GetParameters().Length > 0
                                  && cctor.GetParameters().Any(parameter => !parameter.ParameterType.IsPrimitive()));
            }
        }

        private static void VerifyModifiers(
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            types = types
                .Where(type => type.IsConcreteType())
                .Where(type => !type.IsRecord());

            foreach (var type in types)
            {
                exceptions.Add(new InvalidOperationException($"Type {type} should be defined as record"));
            }
        }

        private static void VerifyPropertyInitializers(
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            var properties = types
                .Where(type => type.IsConcreteType())
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty))
                .Where(property => !property.HasAttribute<JsonIgnoreAttribute>())
                .Where(property => !property.IsEqualityContract())
                .Where(property => !(property.HasInitializer() && property.SetIsAccessible()));

            foreach (var property in properties)
            {
                exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable"));
            }
        }

        private static void VerifyTypeArguments(
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            types = types
                .Where(type => type.IsConcreteType())
                .Where(type => type.IsGenericTypeDefinition || type.IsPartiallyClosed());

            foreach (var type in types)
            {
                exceptions.Add(new InvalidOperationException($"Type {type} should not have generic arguments so as to be deserializable as part of IntegrationMessage payload"));
            }
        }

        private void VerifyMessageHandlersLifestyle(ICollection<Exception> exceptions)
        {
            var lifestyleViolations = _registrations
                .Resolvable()
                .Where(info => info.Lifestyle != EnLifestyle.Transient)
                .RegisteredComponents()
                .Where(info => info.IsSubclassOfOpenGeneric(typeof(IMessageHandler<>)));

            foreach (var lifestyleViolation in lifestyleViolations)
            {
                exceptions.Add(new InvalidOperationException($"Message handler {lifestyleViolation.FullName} should have transient lifestyle"));
            }
        }

        private void VerifyHandlerExistence(
            IEnumerable<Type> types,
            ICollection<Exception> exceptions)
        {
            types = types
                .Where(type => type.IsConcreteType())
                .Where(type => !type.HasMessageHandler(_registrations));

            foreach (var type in types)
            {
                exceptions.Add(new InvalidOperationException($"Message '{type.FullName}' should have at least one message handler"));
            }
        }
    }
}