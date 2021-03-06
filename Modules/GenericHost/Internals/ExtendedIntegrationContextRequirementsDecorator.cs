namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using Basics;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;
    using Core.GenericEndpoint.Contract.Abstractions;

    [ManualRegistration]
    internal class ExtendedIntegrationContextRequirementsDecorator : IExtendedIntegrationContext,
                                                                     IDecorator<IExtendedIntegrationContext>
    {
        private const string EndpointScopeRequired = "Method should be processed within endpoint scope (in message handler)";

        public ExtendedIntegrationContextRequirementsDecorator(IExtendedIntegrationContext decoratee)
        {
            Decoratee = decoratee;
        }

        public IExtendedIntegrationContext Decoratee { get; }

        public IntegrationMessage Message => Decoratee.Message;

        public EndpointIdentity EndpointIdentity => Decoratee.EndpointIdentity;

        public void Initialize(EndpointRuntimeInfo info)
        {
            Decoratee.Initialize(info);
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Decoratee.Send(command, token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Decoratee.Publish(integrationEvent, token);
        }

        public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            RequireEndpointScope();
            return Decoratee.Request<TQuery, TReply>(query, token);
        }

        public Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            RequireEndpointScope();
            return Decoratee.Reply(query, reply, token);
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            RequireEndpointScope();
            return Decoratee.Retry(dueTime, token);
        }

        public IAsyncDisposable WithinEndpointScope(AsyncUnitOfWorkBuilder<EndpointIdentity> unitOfWorkBuilder)
        {
            return Decoratee.WithinEndpointScope(unitOfWorkBuilder);
        }

        private void RequireEndpointScope()
        {
            Decoratee.Message.EnsureNotNull(EndpointScopeRequired);
            Decoratee.EndpointIdentity.EnsureNotNull(EndpointScopeRequired);
        }
    }
}