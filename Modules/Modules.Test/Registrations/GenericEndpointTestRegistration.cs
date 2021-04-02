namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;

    internal class GenericEndpointTestRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var endpointIdentity = new EndpointIdentity("mock_endpoint", 0);
            container.RegisterInstance(endpointIdentity);

            container.Register<IAdvancedIntegrationContext, AdvancedIntegrationContextMock>();
            container.Register<AdvancedIntegrationContextMock, AdvancedIntegrationContextMock>();
        }

        [Component(EnLifestyle.Scoped, EnComponentKind.ManuallyRegistered)]
        private class AdvancedIntegrationContextMock : IAdvancedIntegrationContext
        {
            private IntegrationMessage? _message;

            public AdvancedIntegrationContextMock(
                EndpointIdentity endpointIdentity,
                IIntegrationUnitOfWork unitOfWork)
            {
                EndpointIdentity = endpointIdentity;
                UnitOfWork = unitOfWork;
            }

            public IntegrationMessage Message => _message ?? throw new ArgumentNullException(nameof(_message));

            public EndpointIdentity EndpointIdentity { get; }

            public IIntegrationUnitOfWork UnitOfWork { get; }

            public void Initialize(IntegrationMessage inputData)
            {
                _message = inputData;
            }

            public Task Send<TCommand>(TCommand command, CancellationToken token)
                where TCommand : IIntegrationCommand
            {
                return Task.CompletedTask;
            }

            public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
                where TEvent : IIntegrationEvent
            {
                return Task.CompletedTask;
            }

            public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
                where TQuery : IIntegrationQuery<TReply>
                where TReply : IIntegrationMessage
            {
                return Task.CompletedTask;
            }

            public Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
                where TQuery : IIntegrationQuery<TReply>
                where TReply : IIntegrationMessage
            {
                return Task.CompletedTask;
            }

            public Task Retry(TimeSpan dueTime, CancellationToken token)
            {
                return Task.CompletedTask;
            }

            public Task DeliverAll(CancellationToken token)
            {
                return Task.CompletedTask;
            }
        }
    }
}