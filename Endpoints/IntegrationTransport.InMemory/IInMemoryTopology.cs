namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    internal interface IInMemoryTopology
    {
        public IEnumerable<(Func<IntegrationMessage, Task>? MessageHandler, Type ReflectedType, string? Reason)> Dispatch(IntegrationMessage message);

        Task<(bool IsSuccess, string? Reason)> TryHandleError(
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token);

        public void BindMessageHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
            IReadOnlyCollection<Type> messageTypes);

        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler);

        public void Lock(EndpointIdentity endpointIdentity);
    }
}