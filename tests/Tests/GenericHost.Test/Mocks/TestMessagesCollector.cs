namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics.Primitives;
    using CrossCuttingConcerns.Logging;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using IntegrationTransport.Api.Abstractions;
    using Microsoft.Extensions.Logging;

    [ManuallyRegisteredComponent(nameof(EndpointCommunicationTests))]
    internal class TestMessagesCollector : IResolvable<TestMessagesCollector>,
                                           IDisposable
    {
        private readonly IExecutableIntegrationTransport _integrationTransport;
        private readonly ILogger _logger;

        private readonly EventHandler<IntegrationTransportMessageReceivedEventArgs> _subscription;

        public TestMessagesCollector(
            IExecutableIntegrationTransport integrationTransport,
            ILogger logger)
        {
            _integrationTransport = integrationTransport;
            _logger = logger;

            _subscription = Collect;

            integrationTransport.MessageReceived += _subscription;

            Messages = new ConcurrentQueue<IntegrationMessage>();
            ErrorMessages = new ConcurrentQueue<(IntegrationMessage, Exception?)>();
        }

        private event EventHandler<MessageCollectedEventArgs>? OnCollected;

        public ConcurrentQueue<IntegrationMessage> Messages { get; }

        public ConcurrentQueue<(IntegrationMessage message, Exception? exception)> ErrorMessages { get; }

        public void Dispose()
        {
            _integrationTransport.MessageReceived -= _subscription;

            Messages.Clear();
            ErrorMessages.Clear();
        }

        public async Task WaitUntilErrorMessageIsNotReceived(
            Predicate<IntegrationMessage>? messagePredicate = null,
            Predicate<Exception>? exceptionPredicate = null)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var subscription = MakeSubscription(
                messagePredicate ?? (_ => true),
                exceptionPredicate ?? (_ => true),
                tcs);

            using (Disposable.Create((this, subscription), Subscribe, Unsubscribe))
            {
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<MessageCollectedEventArgs> MakeSubscription(
                Predicate<IntegrationMessage> predicate,
                Predicate<Exception> exceptionPredicate,
                TaskCompletionSource tcs)
            {
                return (_, eventArgs) =>
                {
                    if (eventArgs.Exception != null
                        && exceptionPredicate(eventArgs.Exception)
                        && predicate(eventArgs.Message))
                    {
                        _ = tcs.TrySetResult();
                    }
                };
            }
        }

        public async Task WaitUntilErrorMessageIsNotReceived<TMessage>(
            Predicate<TMessage>? messagePredicate = null,
            Predicate<Exception>? exceptionPredicate = null)
            where TMessage : IIntegrationMessage
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var subscription = MakeSubscription(
                messagePredicate ?? (_ => true),
                exceptionPredicate ?? (_ => true),
                tcs);

            using (Disposable.Create((this, subscription), Subscribe, Unsubscribe))
            {
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<MessageCollectedEventArgs> MakeSubscription(
                Predicate<TMessage> predicate,
                Predicate<Exception> exceptionPredicate,
                TaskCompletionSource tcs)
            {
                return (_, eventArgs) =>
                {
                    if (eventArgs.Exception != null
                        && exceptionPredicate(eventArgs.Exception)
                        && eventArgs.Message.Payload is TMessage message
                        && predicate(message))
                    {
                        _ = tcs.TrySetResult();
                    }
                };
            }
        }

        public async Task WaitUntilMessageIsNotReceived(Predicate<IntegrationMessage>? predicate = null)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var subscription = MakeSubscription(predicate ?? (_ => true), tcs);

            using (Disposable.Create((this, subscription), Subscribe, Unsubscribe))
            {
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<MessageCollectedEventArgs> MakeSubscription(
                Predicate<IntegrationMessage> predicate,
                TaskCompletionSource tcs)
            {
                return (_, eventArgs) =>
                {
                    if (predicate(eventArgs.Message))
                    {
                        _ = tcs.TrySetResult();
                    }
                };
            }
        }

        public async Task WaitUntilMessageIsNotReceived<TMessage>(Predicate<TMessage>? predicate = null)
            where TMessage : IIntegrationMessage
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var subscription = MakeSubscription(predicate ?? (_ => true), tcs);

            using (Disposable.Create((this, subscription), Subscribe, Unsubscribe))
            {
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<MessageCollectedEventArgs> MakeSubscription(
                Predicate<TMessage> predicate,
                TaskCompletionSource tcs)
            {
                return (_, eventArgs) =>
                {
                    if (eventArgs.Message.Payload is TMessage message
                        && predicate(message))
                    {
                        _ = tcs.TrySetResult();
                    }
                };
            }
        }

        private void Collect(object? sender, IntegrationTransportMessageReceivedEventArgs args)
        {
            if (args.Exception == null)
            {
                _logger.Information($"Message has been collected: {args.Message.Payload.GetType().Name};");
                Messages.Enqueue(args.Message);
            }
            else
            {
                _logger.Information($"Failed message has been collected: {args.Message.Payload.GetType().Name};");
                ErrorMessages.Enqueue((args.Message, args.Exception));
            }

            OnCollected?.Invoke(this, new MessageCollectedEventArgs(args.Message, args.Exception));
        }

        private static void Subscribe((TestMessagesCollector, EventHandler<MessageCollectedEventArgs>) state)
        {
            var (collector, subscription) = state;
            collector.OnCollected += subscription;
        }

        private static void Unsubscribe((TestMessagesCollector, EventHandler<MessageCollectedEventArgs>) state)
        {
            var (collector, subscription) = state;
            collector.OnCollected -= subscription;
        }

        private class MessageCollectedEventArgs : EventArgs
        {
            public MessageCollectedEventArgs(IntegrationMessage message, Exception? exception)
            {
                Message = message;
                Exception = exception;
            }

            public IntegrationMessage Message { get; }

            public Exception? Exception { get; }
        }
    }
}