namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Primitives;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent("We need register instance of this class so as to collect and assert messages inside test")]
    internal class MessagesCollector : IResolvable<MessagesCollector>
    {
        public MessagesCollector()
        {
            Messages = new ConcurrentQueue<IntegrationMessage>();
            ErrorMessages = new ConcurrentQueue<(IntegrationMessage, Exception?)>();
        }

        private event EventHandler<MessageCollectedEventArgs>? OnCollected;

        public ConcurrentQueue<IntegrationMessage> Messages { get; }

        public ConcurrentQueue<(IntegrationMessage message, Exception? exception)> ErrorMessages { get; }

        [SuppressMessage("Analysis", "CA1801", Justification = "test mock integration")]
        public Task Collect(IntegrationMessage message, Exception? exception, CancellationToken token)
        {
            if (exception == null)
            {
                Messages.Enqueue(message);
            }
            else
            {
                ErrorMessages.Enqueue((message, exception));
            }

            OnCollected?.Invoke(this, new MessageCollectedEventArgs(message, exception));

            return Task.CompletedTask;
        }

        public async Task WaitUntilErrorMessageIsNotReceived(
            Predicate<IntegrationMessage>? messagePredicate = null,
            Predicate<Exception>? exceptionPredicate = null)
        {
            var tcs = new TaskCompletionSource();
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
                        tcs.SetResult();
                    }
                };
            }
        }

        public async Task WaitUntilErrorMessageIsNotReceived<TMessage>(
            Predicate<TMessage>? messagePredicate = null,
            Predicate<Exception>? exceptionPredicate = null)
            where TMessage : IIntegrationMessage
        {
            var tcs = new TaskCompletionSource();
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
                        tcs.SetResult();
                    }
                };
            }
        }

        public async Task WaitUntilMessageIsNotReceived(Predicate<IntegrationMessage>? predicate = null)
        {
            var tcs = new TaskCompletionSource();
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
                        tcs.SetResult();
                    }
                };
            }
        }

        public async Task WaitUntilMessageIsNotReceived<TMessage>(Predicate<TMessage>? predicate = null)
            where TMessage : IIntegrationMessage
        {
            var tcs = new TaskCompletionSource();
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
                        tcs.SetResult();
                    }
                };
            }
        }

        private static void Subscribe((MessagesCollector, EventHandler<MessageCollectedEventArgs>) state)
        {
            var (collector, subscription) = state;
            collector.OnCollected += subscription;
        }

        private static void Unsubscribe((MessagesCollector, EventHandler<MessageCollectedEventArgs>) state)
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