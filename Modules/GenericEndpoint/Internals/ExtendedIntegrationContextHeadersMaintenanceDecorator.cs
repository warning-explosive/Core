namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericEndpoint;

    [Component(EnLifestyle.Scoped)]
    internal class ExtendedIntegrationContextHeadersMaintenanceDecorator : IExtendedIntegrationContext,
                                                                           IDecorator<IExtendedIntegrationContext>
    {
        public ExtendedIntegrationContextHeadersMaintenanceDecorator(IExtendedIntegrationContext decoratee)
        {
            Decoratee = decoratee;
        }

        public IExtendedIntegrationContext Decoratee { get; }

        public IntegrationMessage Message => Decoratee.Message;

        public EndpointIdentity EndpointIdentity => Decoratee.EndpointIdentity;

        public IIntegrationUnitOfWork UnitOfWork => Decoratee.UnitOfWork;

        public void Initialize(IntegrationMessage message)
        {
            Decoratee.Initialize(message);
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
            return Decoratee.Request<TQuery, TReply>(query, token);
        }

        public Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            Message.SetReplied();

            return Decoratee.Reply(query, reply, token);
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            Message.IncrementRetryCounter();

            return Decoratee.Retry(dueTime, token);
        }

        public Task DeliverAll(CancellationToken token)
        {
            return Decoratee.DeliverAll(token);
        }
    }
}