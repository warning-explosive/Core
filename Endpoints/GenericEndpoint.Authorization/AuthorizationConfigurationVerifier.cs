namespace SpaceEngineers.Core.GenericEndpoint.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Verifiers;
    using Contract;
    using Contract.Attributes;
    using Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class AuthorizationConfigurationVerifier : IConfigurationVerifier,
                                                        ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly IIntegrationTypeProvider _integrationTypeProvider;

        public AuthorizationConfigurationVerifier(IIntegrationTypeProvider integrationTypeProvider)
        {
            _integrationTypeProvider = integrationTypeProvider;
        }

        public void Verify()
        {
            var exceptions = new List<Exception>();

            VerifyFeatureAttribute(_integrationTypeProvider.IntegrationMessageTypes(), exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void VerifyFeatureAttribute(
            IEnumerable<Type> messageTypes,
            ICollection<Exception> exceptions)
        {
            foreach (var messageType in messageTypes)
            {
                if (messageType.IsMessageContractAbstraction())
                {
                    continue;
                }

                if (!messageType.HasAttribute<FeatureAttribute>())
                {
                    exceptions.Add(new InvalidOperationException($"Message {messageType.FullName} should be marked by {nameof(FeatureAttribute)} in order to provide authorization capabilities"));
                }
            }
        }
    }
}