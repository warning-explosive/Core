namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Primitives;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;

    [ManuallyRegisteredComponent("We need register instance of this class so as to collect and assert messages inside test")]
    internal class MessagesCollector : IResolvable
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

            OnCollected?.Invoke(this, new MessageCollectedEventArgs(message));

            return Task.CompletedTask;
        }

        public async Task WaitUntilMessageIsNotReceived<TMessage>(Predicate<TMessage> predicate)
            where TMessage : IIntegrationMessage
        {
            var tcs = new TaskCompletionSource();
            var subscription = MakeSubscription(predicate, tcs);

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

            static void Subscribe((MessagesCollector, EventHandler<MessageCollectedEventArgs>) state)
            {
                var (collector, subscription) = state;
                collector.OnCollected += subscription;
            }

            static void Unsubscribe((MessagesCollector, EventHandler<MessageCollectedEventArgs>) state)
            {
                var (collector, subscription) = state;
                collector.OnCollected -= subscription;
            }
        }

        public void ShowAllMessages(Action<string> log)
        {
            Messages.Each(message => log(message.ToString()));
        }

        public void ShowAllErrorMessages(Action<string> log)
        {
            ErrorMessages.Each(message => log($"{message.message} [Exception, {message.exception?.Message}]"));
        }

        private class MessageCollectedEventArgs : EventArgs
        {
            public MessageCollectedEventArgs(IntegrationMessage message)
            {
                Message = message;
            }

            public IntegrationMessage Message { get; }
        }
    }
}