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
            var exceptions = new List<Exception>();

            VerifyEndpointLogicalName(_endpointIdentity, exceptions);

            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EndpointCommands(), exceptions);
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EventsSubscriptions(), exceptions);
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.EndpointQueries(), exceptions);
            VerifyMessageNames(_endpointIdentity, _integrationTypeProvider.RepliesSubscriptions(), exceptions);

            VerifyModifiers(_integrationTypeProvider.IntegrationMessageTypes(), exceptions);
            VerifyConstructors(_integrationTypeProvider.IntegrationMessageTypes(), exceptions);
            VerifyOwnedAttribute(_integrationTypeProvider.IntegrationMessageTypes(), exceptions);
            VerifyMessageInterfaces(_integrationTypeProvider.IntegrationMessageTypes(), exceptions);
            VerifyPropertyInitializers(_integrationTypeProvider.IntegrationMessageTypes(), exceptions);

            VerifyHandlerExistence(_integrationTypeProvider.EndpointCommands().Where(type => type.IsConcreteType()), exceptions);
            VerifyHandlerExistence(_integrationTypeProvider.EndpointQueries().Where(type => type.IsConcreteType()), exceptions);

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
            IEnumerable<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            foreach (var messageType in messageTypes.Where(type => HasWrongName(endpointIdentity, type)))
            {
                exceptions.Add(new InvalidOperationException($"Message name {messageType.FullName} should be less or equal than 255 bytes"));
            }

            static bool HasWrongName(EndpointIdentity endpointIdentity, Type type)
            {
                var left = type.GenericTypeDefinitionOrSelf().FullName!;
                var right = endpointIdentity.LogicalName;

                return left.Length + right.Length + 1 > 255;
            }
        }

        private static void VerifyMessageInterfaces(
            IEnumerable<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            foreach (var messageType in messageTypes.Where(ImplementsSeveralSpecializedInterfaces().Not()))
            {
                exceptions.Add(new InvalidOperationException($"Message {messageType.FullName} must implement only one specialized interface (command, query, event or just message)"));
            }

            static Func<Type, bool> ImplementsSeveralSpecializedInterfaces()
            {
                return type =>
                {
                    var sum = type.IsCommand().Bit()
                              + type.IsEvent().Bit()
                              + type.IsQuery().Bit()
                              + type.IsReply().Bit();

                    return sum == 1;
                };
            }
        }

        private static void VerifyOwnedAttribute(
            IReadOnlyCollection<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            foreach (var messageType in messageTypes)
            {
                if (messageType.IsMessageContractAbstraction())
                {
                    continue;
                }

                if (messageType.IsReply()
                    && !messageTypes.Any(type => messageType.IsReplyOnQuery(type)))
                {
                    exceptions.Add(new InvalidOperationException($"Reply {messageType.FullName} should have at least one corresponding {nameof(IIntegrationQuery<IIntegrationReply>)}"));
                }

                if (messageType.IsReply()
                    && messageType.HasAttribute<OwnedByAttribute>())
                {
                    exceptions.Add(new InvalidOperationException($"Reply {messageType.FullName} should not be marked by {nameof(OwnedByAttribute)}"));
                }

                if (!messageType.IsReply()
                    && !messageType.HasAttribute<OwnedByAttribute>())
                {
                    exceptions.Add(new InvalidOperationException($"Message {messageType.FullName} should be marked by {nameof(OwnedByAttribute)} in order to provide automatic service discovery"));
                }
            }
        }

        private void VerifyConstructors(
            IEnumerable<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            foreach (var messageType in messageTypes.Where(type => HasWrongConstructors(type, _constructorResolutionBehavior)))
            {
                exceptions.Add(new InvalidOperationException($"Message {messageType.FullName} should have one public constructor"));
            }

            static bool HasWrongConstructors(Type messageType, IConstructorResolutionBehavior constructorResolutionBehavior)
            {
                return messageType.IsConcreteType() && !constructorResolutionBehavior.TryGetConstructor(messageType, out _);
            }
        }

        private static void VerifyModifiers(
            IEnumerable<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            foreach (var messageType in messageTypes.Where(messageType => !messageType.IsRecord()))
            {
                exceptions.Add(new InvalidOperationException($"Type {messageType} should be defined as record"));
            }
        }

        private static void VerifyPropertyInitializers(
            IEnumerable<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            var properties = messageTypes
                .Where(messageType => messageType.IsConcreteType())
                .SelectMany(messageType => messageType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty))
                .Where(property => !property.IsEqualityContract())
                .Where(property => !(property.HasInitializer() && property.SetIsAccessible()));

            foreach (var property in properties)
            {
                exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable"));
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
            IEnumerable<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            foreach (var messageType in messageTypes.Where(messageType => !messageType.HasMessageHandler(_registrations)))
            {
                exceptions.Add(new InvalidOperationException($"Message '{messageType.FullName}' should have at least one message handler"));
            }
        }
    }
}