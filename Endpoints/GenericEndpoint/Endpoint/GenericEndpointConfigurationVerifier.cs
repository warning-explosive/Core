namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot.Extensions;
    using CompositionRoot.Registration;
    using CompositionRoot.Verifiers;
    using Contract;
    using Contract.Abstractions;
    using Contract.Attributes;
    using Extensions;
    using Messaging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier,
                                                          ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly IRegistrationsContainer _registrations;

        public GenericEndpointConfigurationVerifier(
            EndpointIdentity endpointIdentity,
            IConstructorResolutionBehavior constructorResolutionBehavior,
            IIntegrationTypeProvider integrationTypeProvider,
            IRegistrationsContainer registrations)
        {
            _endpointIdentity = endpointIdentity;
            _constructorResolutionBehavior = constructorResolutionBehavior;
            _integrationTypeProvider = integrationTypeProvider;
            _registrations = registrations;
        }

        /// <inheritdoc />
        public void Verify()
        {
            VerifyLogicalName(_endpointIdentity);

            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EndpointCommands());
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EventsSubscriptions());
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EndpointQueries());
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.RepliesSubscriptions());

            VerifyMessageInterfaces(_integrationTypeProvider.IntegrationMessageTypes());
            VerifyOwnedAttribute(_integrationTypeProvider.IntegrationMessageTypes());

            VerifyConstructors(_integrationTypeProvider.IntegrationMessageTypes());
            MessageTypesHaveMissingPropertyInitializers(_integrationTypeProvider.IntegrationMessageTypes());

            VerifyMessageHandlersLifestyle();

            VerifyHandlerExistence(_integrationTypeProvider.EndpointCommands().Where(type => type.IsConcreteType()));
            VerifyHandlerExistence(_integrationTypeProvider.EndpointQueries().Where(type => type.IsConcreteType()));
        }

        private static void VerifyLogicalName(EndpointIdentity endpointIdentity)
        {
            var pattern = new Regex("[^a-zA-Z\\d]", RegexOptions.Compiled);

            if (pattern.IsMatch(endpointIdentity.LogicalName))
            {
                throw new InvalidOperationException($"Endpoint {endpointIdentity} should have only letters in logical name");
            }
        }

        private static void VerifyMessageNames(EndpointIdentity endpointIdentity, IEnumerable<Type> messageTypes)
        {
            messageTypes
                .Where(type => HasWrongName(endpointIdentity, type))
                .Each(type => throw new InvalidOperationException($"Message name {type.FullName} should be less or equal than 255 bytes"));

            static bool HasWrongName(EndpointIdentity endpointIdentity, Type type)
            {
                var left = type.FullName!;
                var right = endpointIdentity.LogicalName;

                return left.Length + right.Length + 1 > 255;
            }
        }

        private static void VerifyMessageInterfaces(IEnumerable<Type> messageTypes)
        {
            messageTypes
                .Where(ImplementsSeveralSpecializedInterfaces)
                .Each(type => throw new InvalidOperationException($"Message {type.FullName} must implement only one specialized interface (command, query, event or just message)"));

            static bool ImplementsSeveralSpecializedInterfaces(Type type)
            {
                var sum = typeof(IIntegrationCommand).IsAssignableFrom(type).Bit()
                          + typeof(IIntegrationEvent).IsAssignableFrom(type).Bit()
                          + typeof(IIntegrationReply).IsAssignableFrom(type).Bit()
                          + type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>)).Bit();

                return sum > 1;
            }
        }

        private static void VerifyOwnedAttribute(IReadOnlyCollection<Type> messageTypes)
        {
            foreach (var messageType in messageTypes)
            {
                if (messageType.IsMessageContractAbstraction())
                {
                    continue;
                }

                if (!typeof(IIntegrationReply).IsAssignableFrom(messageType))
                {
                    _ = messageType.GetRequiredAttribute<OwnedByAttribute>();
                }
                else
                {
                    if (messageType.HasAttribute<OwnedByAttribute>())
                    {
                        throw new InvalidOperationException($"Reply should not have {nameof(OwnedByAttribute)}");
                    }

                    var integrationQuery = typeof(IIntegrationQuery<>).MakeGenericType(messageType);

                    var queryTypes = messageTypes
                       .Where(type => integrationQuery.IsAssignableFrom(type))
                       .ToList();

                    if (queryTypes.Count == 0)
                    {
                        throw new InvalidOperationException($"Reply should have at least one corresponding {nameof(IIntegrationQuery<IIntegrationReply>)}");
                    }
                }
            }
        }

        private void VerifyConstructors(IEnumerable<Type> messageTypes)
        {
            messageTypes
               .Where(messageType => messageType.IsConcreteType())
               .Where(messageType => !_constructorResolutionBehavior.TryGetConstructor(messageType, out _))
               .Each(messageType => throw new InvalidOperationException($"Message {messageType.FullName} should have one public constructor"));
        }

        private static void MessageTypesHaveMissingPropertyInitializers(IEnumerable<Type> messageTypes)
        {
            messageTypes
               .Where(messageType => messageType.IsConcreteType())
               .SelectMany(messageType => messageType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty))
               .Where(property => !(property.HasInitializer() && property.SetIsAccessible()))
               .Each(property => throw new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable"));
        }

        private void VerifyMessageHandlersLifestyle()
        {
            var lifestyleViolations = _registrations
                .Resolvable()
                .Where(info => info.Lifestyle != EnLifestyle.Transient)
                .RegisteredComponents()
                .Where(info => info.IsSubclassOfOpenGeneric(typeof(IMessageHandler<>)))
                .ToList();

            if (lifestyleViolations.Any())
            {
                throw new InvalidOperationException($"Message handlers {lifestyleViolations.ToString(", ", type => type.FullName)} should have transient lifestyle");
            }
        }

        private void VerifyHandlerExistence(IEnumerable<Type> messageTypes)
        {
            foreach (var messageType in messageTypes)
            {
                if (!messageType.HasMessageHandler(_registrations))
                {
                    throw new InvalidOperationException($"Message '{messageType.FullName}' should have at least one message handler");
                }
            }
        }
    }
}