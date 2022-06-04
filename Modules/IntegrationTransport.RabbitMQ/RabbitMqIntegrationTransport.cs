namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Primitives;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Messaging.MessageHeaders;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;
    using Microsoft.Extensions.Logging;
    using Settings;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [ManuallyRegisteredComponent("We have isolation between several endpoints. Each of them have their own DependencyContainer. We need to pass the same instance of transport into all DI containers.")]
    internal class RabbitMqIntegrationTransport : IIntegrationTransport,
                                                  IResolvable<IIntegrationTransport>,
                                                  IDisposable
    {
        public const string ContentEncoding = "gzip";

        private const string InputExchange = nameof(InputExchange);

        private const string DeadLetterExchange = nameof(DeadLetterExchange);
        private const string DeadLetterQueue = nameof(DeadLetterQueue);

        private const string DeferredExchange = nameof(DeferredExchange);
        private const string DeferredQueue = nameof(DeferredQueue);

        private readonly ILogger _logger;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ISettingsProvider<RabbitMqSettings> _rabbitMqSettingsProvider;

        private readonly ConcurrentDictionary<EndpointIdentity, object?> _endpoints;
        private readonly ConcurrentDictionary<EndpointIdentity, IIntegrationTypeProvider> _integrationMessageTypes;

        private readonly ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>> _messageHandlers;
        private readonly ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>> _errorMessageHandlers;

        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> _outstandingConfirms;

        private readonly AsyncManualResetEvent _ready;

        private EnIntegrationTransportStatus _status;

        private IConnection? _connection;
        private IReadOnlyDictionary<EndpointIdentity, IModel>? _channels;

        public RabbitMqIntegrationTransport(
            ILogger logger,
            EndpointIdentity endpointIdentity,
            IJsonSerializer jsonSerializer,
            ISettingsProvider<RabbitMqSettings> rabbitMqSettingsProvider)
        {
            _logger = logger;
            _endpointIdentity = endpointIdentity;
            _jsonSerializer = jsonSerializer;
            _rabbitMqSettingsProvider = rabbitMqSettingsProvider;

            _endpoints = new ConcurrentDictionary<EndpointIdentity, object?>();
            _integrationMessageTypes = new ConcurrentDictionary<EndpointIdentity, IIntegrationTypeProvider>();

            _messageHandlers = new ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>>();
            _errorMessageHandlers = new ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>>();

            _outstandingConfirms = new ConcurrentDictionary<ulong, TaskCompletionSource<bool>>();

            _ready = new AsyncManualResetEvent(false);

            _status = EnIntegrationTransportStatus.Stopped;

            _channels = new Dictionary<EndpointIdentity, IModel>();
        }

        public event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        public EnIntegrationTransportStatus Status
        {
            get => _status;

            private set
            {
                var previousValue = _status;
                _status = value;
                var eventArgs = new IntegrationTransportStatusChangedEventArgs(previousValue, value);
                StatusChanged?.Invoke(this, eventArgs);
            }
        }

        public void Dispose()
        {
            foreach (var (_, channel) in _channels)
            {
                channel.Close();
                channel.Dispose();
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        public void Bind(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, CancellationToken, Task> messageHandler,
            IIntegrationTypeProvider integrationTypeProvider)
        {
            _endpoints.TryAdd(endpointIdentity, default);

            _integrationMessageTypes.TryAdd(endpointIdentity, integrationTypeProvider);
            _messageHandlers.TryAdd(endpointIdentity, messageHandler);
        }

        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler)
        {
            _endpoints.TryAdd(endpointIdentity, default);

            var endpointErrorHandlers = _errorMessageHandlers.GetOrAdd(endpointIdentity, new ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>());
            endpointErrorHandlers.Add(errorMessageHandler);
        }

        public async Task<bool> Enqueue(IntegrationMessage message, CancellationToken token)
        {
            await _ready.WaitAsync(token).ConfigureAwait(false);

            var channel = _channels[message.ReadRequiredHeader<SentFrom>().Value];

            var basicProperties = CreateBasicProperties(channel, message, out var exchange, out var routingKey);

            var body = message.EncodeIntegrationMessage(_jsonSerializer);

            Task<bool> confirmedPublication;

            lock (channel)
            {
                confirmedPublication = _outstandingConfirms
                   .GetOrAdd(channel.NextPublishSeqNo, _ => new TaskCompletionSource<bool>())
                   .Task;

                channel.BasicPublish(exchange, routingKey, true, basicProperties, body);
            }

            return await confirmedPublication.ConfigureAwait(false);

            static IBasicProperties CreateBasicProperties(
                IModel channel,
                IntegrationMessage message,
                out string exchange,
                out string routingKey)
            {
                var basicProperties = channel.CreateBasicProperties();

                basicProperties.DeliveryMode = 2;

                var deferredUntil = message.ReadHeader<DeferredUntil>();
                var expirationMilliseconds = (ulong)Math.Round((deferredUntil?.Value - DateTime.UtcNow)?.TotalMilliseconds ?? 0.0, MidpointRounding.AwayFromZero);

                var isDeferred = expirationMilliseconds > 0;

                if (isDeferred)
                {
                    basicProperties.Expiration = expirationMilliseconds.ToString(CultureInfo.InvariantCulture);
                }

                basicProperties.ContentType = MediaTypeNames.Application.Json;
                basicProperties.ContentEncoding = ContentEncoding;

                basicProperties.AppId = message.ReadRequiredHeader<SentFrom>().Value.LogicalName;
                basicProperties.ClusterId = message.ReadRequiredHeader<SentFrom>().Value.InstanceName;
                basicProperties.MessageId = message.ReadRequiredHeader<Id>().Value.ToString();
                basicProperties.CorrelationId = message.ReadRequiredHeader<ConversationId>().Value.ToString();
                basicProperties.Type = message.ReflectedType.FullName;
                basicProperties.Headers = new Dictionary<string, object>();
                basicProperties.ReplyTo = message.ReadHeader<ReplyTo>()?.Value.LogicalName ?? default;

                exchange = isDeferred ? DeferredExchange : InputExchange;
                routingKey = message.ReflectedType.FullName!;

                return basicProperties;
            }
        }

        public Task EnqueueError(
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token)
        {
            if (_errorMessageHandlers.TryGetValue(endpointIdentity, out var handlers))
            {
                return Task.WhenAll(handlers.Select(handler => handler(message.Clone(), exception, token)));
            }

            throw new InvalidOperationException($"Unable to process error message. Please register error handler for {endpointIdentity} endpoint.");
        }

        public Task Accept(IntegrationMessage message, CancellationToken token)
        {
            message.Ack(_channels[message.ReadRequiredHeader<HandledBy>().Value]);

            return Task.CompletedTask;
        }

        public async Task StartBackgroundMessageProcessing(CancellationToken token)
        {
            Status = EnIntegrationTransportStatus.Starting;

            var rabbitMqSettings = await _rabbitMqSettingsProvider
               .Get(token)
               .ConfigureAwait(false);

            await rabbitMqSettings
               .DeclareVirtualHost(_jsonSerializer, token)
               .ConfigureAwait(false);

            var backgroundMessageProcessingTaskCompletionSource = new TaskCompletionSource<object?>();

            _connection = await ConfigureConnection(
                    _logger,
                    rabbitMqSettings,
                    HandleConnectionShutdown(_logger, _ready, backgroundMessageProcessingTaskCompletionSource),
                    token)
               .ConfigureAwait(false);

            var handleChannelShutdownSubscription = HandleChannelShutdown(_logger, _ready, backgroundMessageProcessingTaskCompletionSource);
            var handleChannelCallbackExceptionSubscription = HandleChannelCallbackException(_logger, _ready, backgroundMessageProcessingTaskCompletionSource);

            var consumers = new Dictionary<string, EndpointIdentity>(_channels.Count, StringComparer.Ordinal);
            var handleChannelBasicReturnSubscription = HandleChannelBasicReturn(_logger, EnqueueError, consumers, _jsonSerializer, token);

            var handleChannelBasicAcksSubscription = HandleChannelBasicAcks();
            var handleChannelBasicNacksSubscription = HandleChannelBasicNacks();

            using var commonChannel = ConfigureChannel(
                _connection,
                rabbitMqSettings,
                handleChannelShutdownSubscription,
                handleChannelCallbackExceptionSubscription,
                handleChannelBasicReturnSubscription,
                handleChannelBasicAcksSubscription,
                handleChannelBasicNacksSubscription);

            BuildTopology(
                commonChannel,
                rabbitMqSettings,
                _endpoints.Keys,
                _integrationMessageTypes);

            _channels = ConfigureChannels(
                _connection,
                _endpoints.Keys,
                rabbitMqSettings,
                handleChannelShutdownSubscription,
                handleChannelCallbackExceptionSubscription,
                handleChannelBasicReturnSubscription,
                handleChannelBasicAcksSubscription,
                handleChannelBasicNacksSubscription);

            ConfigureErrorHandlers(
                _channels,
                _endpoints.Keys,
                BindErrorHandler);

            var handleConsumerShutdownSubscription = HandleConsumerShutdown(_logger, _ready, backgroundMessageProcessingTaskCompletionSource);

            StartConsumers(
                _logger,
                _channels,
                consumers,
                _messageHandlers,
                handleConsumerShutdownSubscription,
                _jsonSerializer,
                _ready,
                token);

            _ready.Set();

            Status = EnIntegrationTransportStatus.Running;

            await backgroundMessageProcessingTaskCompletionSource.Task.ConfigureAwait(false);
        }

        #region configuration

        private static async Task<IConnection> ConfigureConnection(
            ILogger logger,
            RabbitMqSettings rabbitMqSettings,
            EventHandler<ShutdownEventArgs> handleConnectionShutdownSubscription,
            CancellationToken token)
        {
            for (var i = 0; i < 4; i++)
            {
                logger.Debug($"Trying to establish connection with RabbitMQ broker: {i}");

                try
                {
                    var connectionFactory = new ConnectionFactory
                    {
                        HostName = rabbitMqSettings.Host,
                        Port = rabbitMqSettings.Port,
                        UserName = rabbitMqSettings.User,
                        Password = rabbitMqSettings.Password,
                        VirtualHost = rabbitMqSettings.VirtualHost,
                        ClientProvidedName = rabbitMqSettings.ApplicationName,
                        DispatchConsumersAsync = true,
                        AutomaticRecoveryEnabled = true,
                        TopologyRecoveryEnabled = false,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                    };

                    var connection = connectionFactory.CreateConnection();

                    connection.ConnectionShutdown += handleConnectionShutdownSubscription;

                    logger.Debug("Connection with RabbitMQ broker was successfully established");

                    return connection;
                }
                catch (BrokerUnreachableException brokerUnreachableException)
                {
                    logger.Error(brokerUnreachableException);
                }

                await Task
                   .Delay(TimeSpan.FromSeconds(15), token)
                   .ConfigureAwait(false);
            }

            throw new BrokerUnreachableException(new InvalidOperationException("Unable to establish connection with RabbitMQ message broker"));
        }

        private static IReadOnlyDictionary<EndpointIdentity, IModel> ConfigureChannels(
            IConnection connection,
            ICollection<EndpointIdentity> endpoints,
            RabbitMqSettings rabbitMqSettings,
            EventHandler<ShutdownEventArgs> handleChannelShutdownSubscription,
            EventHandler<CallbackExceptionEventArgs> handleChannelCallbackExceptionSubscription,
            EventHandler<BasicReturnEventArgs> handleChannelBasicReturnSubscription,
            EventHandler<BasicAckEventArgs> handleChannelBasicAcksSubscription,
            EventHandler<BasicNackEventArgs> handleChannelBasicNacksSubscription)
        {
            return endpoints.ToDictionary(endpointIdentity => endpointIdentity,
                _ => ConfigureChannel(connection,
                    rabbitMqSettings,
                    handleChannelShutdownSubscription,
                    handleChannelCallbackExceptionSubscription,
                    handleChannelBasicReturnSubscription,
                    handleChannelBasicAcksSubscription,
                    handleChannelBasicNacksSubscription));
        }

        private static IModel ConfigureChannel(
            IConnection connection,
            RabbitMqSettings rabbitMqSettings,
            EventHandler<ShutdownEventArgs> handleChannelShutdownSubscription,
            EventHandler<CallbackExceptionEventArgs> handleChannelCallbackExceptionSubscription,
            EventHandler<BasicReturnEventArgs> handleChannelBasicReturnSubscription,
            EventHandler<BasicAckEventArgs> handleChannelBasicAcksSubscription,
            EventHandler<BasicNackEventArgs> handleChannelBasicNacksSubscription)
        {
            var channel = connection.CreateModel();

            channel.ConfirmSelect();
            channel.BasicQos(0, rabbitMqSettings.ChannelPrefetchCount, true);

            channel.ModelShutdown += handleChannelShutdownSubscription;
            channel.CallbackException += handleChannelCallbackExceptionSubscription;

            channel.BasicReturn += handleChannelBasicReturnSubscription;

            channel.BasicAcks += handleChannelBasicAcksSubscription;
            channel.BasicNacks += handleChannelBasicNacksSubscription;

            return channel;
        }

        private static void BuildTopology(
            IModel channel,
            RabbitMqSettings rabbitMqSettings,
            ICollection<EndpointIdentity> endpoints,
            IReadOnlyDictionary<EndpointIdentity, IIntegrationTypeProvider> integrationMessageTypes)
        {
            BuildDeadLetterPath(channel, rabbitMqSettings);
            BuildDeferredPath(channel, rabbitMqSettings);
            BuildInputPath(channel, rabbitMqSettings, endpoints, integrationMessageTypes);

            static void BuildDeadLetterPath(IModel channel, RabbitMqSettings rabbitMqSettings)
            {
                channel.DeclareExchange(
                    exchange: DeadLetterExchange,
                    type: ExchangeType.Direct);

                channel.DeclareQueue(
                    DeadLetterQueue,
                    new Dictionary<string, object>
                    {
                        ["x-queue_type"] = "classic",
                        ["x-max-length-bytes"] = rabbitMqSettings.QueueMaxLengthBytes,
                        ["x-overflow"] = "drop-head"
                    });

                channel.BindQueue(
                    queue: DeadLetterQueue,
                    exchange: DeadLetterExchange);
            }

            static void BuildDeferredPath(IModel channel, RabbitMqSettings rabbitMqSettings)
            {
                channel.DeclareExchange(
                    exchange: DeferredExchange,
                    type: ExchangeType.Direct);

                channel.DeclareQueue(
                    DeferredQueue,
                    new Dictionary<string, object>
                    {
                        ["x-queue_type"] = "classic",
                        ["x-dead-letter-exchange"] = InputExchange,
                        ["x-max-length-bytes"] = rabbitMqSettings.QueueMaxLengthBytes,
                        ["x-overflow"] = "reject-publish"
                    });

                channel.BindQueue(
                    queue: DeferredQueue,
                    exchange: DeferredExchange);
            }

            static void BuildInputPath(
                IModel channel,
                RabbitMqSettings rabbitMqSettings,
                ICollection<EndpointIdentity> endpoints,
                IReadOnlyDictionary<EndpointIdentity, IIntegrationTypeProvider> integrationMessageTypes)
            {
                channel.DeclareExchange(
                    exchange: InputExchange,
                    type: ExchangeType.Direct);

                var distinctIntegrationMessageTypes = integrationMessageTypes
                   .Values
                   .SelectMany(provider => provider.IntegrationMessageTypes())
                   .Distinct();

                foreach (var integrationMessageType in distinctIntegrationMessageTypes)
                {
                    channel.DeclareExchange(
                        integrationMessageType.FullName!,
                        ExchangeType.Direct);

                    channel.BindExchange(
                        source: InputExchange,
                        destination: integrationMessageType.FullName!,
                        routingKey: integrationMessageType.FullName!);
                }

                foreach (var endpointIdentity in endpoints)
                {
                    channel.DeclareQueue(
                        endpointIdentity.LogicalName,
                        new Dictionary<string, object>
                        {
                            ["x-queue_type"] = "classic",
                            ["x-dead-letter-exchange"] = DeadLetterExchange,
                            ["x-max-length-bytes"] = rabbitMqSettings.QueueMaxLengthBytes,
                            ["x-overflow"] = "reject-publish"
                        });
                }

                foreach (var (endpointIdentity, integrationTypeProvider) in integrationMessageTypes)
                {
                    channel.BindQueue(endpointIdentity.LogicalName, integrationTypeProvider.EndpointCommands().Select(messageType => messageType.FullName));
                    channel.BindQueue(endpointIdentity.LogicalName, integrationTypeProvider.EventsSubscriptions().Select(messageType => messageType.FullName));
                    channel.BindQueue(endpointIdentity.LogicalName, integrationTypeProvider.EndpointQueries().Select(messageType => messageType.FullName));
                    channel.BindQueue(endpointIdentity.LogicalName, integrationTypeProvider.RepliesSubscriptions().Select(messageType => messageType.FullName));
                }
            }
        }

        private static void ConfigureErrorHandlers(
            IReadOnlyDictionary<EndpointIdentity, IModel> channels,
            ICollection<EndpointIdentity> endpoints,
            Action<EndpointIdentity, Func<IntegrationMessage, Exception, CancellationToken, Task>> bindErrorHandler)
        {
            foreach (var endpointIdentity in endpoints)
            {
                bindErrorHandler(endpointIdentity,
                    (message, _, _) =>
                    {
                        message.Nack(channels[message.ReadRequiredHeader<HandledBy>().Value]);
                        return Task.CompletedTask;
                    });
            }
        }

        private static void StartConsumers(
            ILogger logger,
            IReadOnlyDictionary<EndpointIdentity, IModel> channels,
            Dictionary<string, EndpointIdentity> consumers,
            IReadOnlyDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>> messageHandlers,
            AsyncEventHandler<ShutdownEventArgs> handleConsumerShutdownSubscription,
            IJsonSerializer jsonSerializer,
            AsyncManualResetEvent ready,
            CancellationToken token)
        {
            var handleReceivedMessageSubscription = HandleReceivedMessage(logger, consumers, messageHandlers, jsonSerializer, ready, token);

            foreach (var (endpointIdentity, channel) in channels)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.Received += handleReceivedMessageSubscription;
                consumer.Shutdown += handleConsumerShutdownSubscription;

                var consumerTag = channel.BasicConsume(endpointIdentity.LogicalName, false, consumer);

                consumers.Add(consumerTag, endpointIdentity);
            }
        }

        #endregion

        #region error event handlers

        private static EventHandler<ShutdownEventArgs> HandleConnectionShutdown(
            ILogger logger,
            AsyncManualResetEvent ready,
            TaskCompletionSource<object?> tcs)
        {
            return (sender, args) =>
            {
                ready.Reset();

                var exception = new InvalidOperationException(args.ToString());

                logger.Error(exception);

                tcs.SetException(exception);
            };
        }

        private static EventHandler<ShutdownEventArgs> HandleChannelShutdown(
            ILogger logger,
            AsyncManualResetEvent ready,
            TaskCompletionSource<object?> tcs)
        {
            return (sender, args) =>
            {
                ready.Reset();

                var exception = new InvalidOperationException(args.ToString());

                logger.Error(exception);

                tcs.SetException(exception);
            };
        }

        private static AsyncEventHandler<ShutdownEventArgs> HandleConsumerShutdown(
            ILogger logger,
            AsyncManualResetEvent ready,
            TaskCompletionSource<object?> tcs)
        {
            return (sender, args) =>
            {
                ready.Reset();

                var exception = new InvalidOperationException(args.ToString());

                logger.Error(exception);

                tcs.SetException(exception);

                return Task.CompletedTask;
            };
        }

        private static EventHandler<CallbackExceptionEventArgs> HandleChannelCallbackException(
            ILogger logger,
            AsyncManualResetEvent ready,
            TaskCompletionSource<object?> tcs)
        {
            return (sender, args) =>
            {
                ready.Reset();

                logger.Error(args.Exception);

                // TODO: #180 - reconnect
                tcs.SetException(args.Exception);
            };
        }

        #endregion

        #region publisher confirms

        private EventHandler<BasicAckEventArgs> HandleChannelBasicAcks()
        {
            return (sender, args) =>
            {
                HandleChannelBasicConfirms(args.DeliveryTag, args.Multiple, true);
            };
        }

        private EventHandler<BasicNackEventArgs> HandleChannelBasicNacks()
        {
            return (sender, args) =>
            {
                HandleChannelBasicConfirms(args.DeliveryTag, args.Multiple, false);
            };
        }

        private void HandleChannelBasicConfirms(
            ulong deliveryTag,
            bool multiple,
            bool result)
        {
            if (multiple)
            {
                var confirmed = _outstandingConfirms
                   .Keys
                   .Where(sequenceNumber => sequenceNumber <= deliveryTag)
                   .ToList();

                foreach (var sequenceNumber in confirmed)
                {
                    Confirm(sequenceNumber, result);
                }
            }
            else
            {
                Confirm(deliveryTag, result);
            }
        }

        private void Confirm(
            ulong deliveryTag,
            bool result)
        {
            if (_outstandingConfirms.TryRemove(deliveryTag, out var tcs))
            {
                tcs.SetResult(result);
            }
        }

        #endregion

        #region delivery_event_handlers

        private static EventHandler<BasicReturnEventArgs> HandleChannelBasicReturn(
            ILogger logger,
            Func<EndpointIdentity, IntegrationMessage, Exception, CancellationToken, Task> enqueueError,
            IReadOnlyDictionary<string, EndpointIdentity> consumers,
            IJsonSerializer jsonSerializer,
            CancellationToken token)
        {
            return (sender, args) =>
            {
                var consumer = (AsyncEventingBasicConsumer)sender;

                ExecutionExtensions
                   .TryAsync((consumer, args, consumers, jsonSerializer, enqueueError), EnqueueReturnedErrorMessage)
                   .Catch<Exception>((exception, _) =>
                   {
                       logger.Error(exception);
                       return Task.CompletedTask;
                   })
                   .Invoke(token)
                   .Wait(token);
            };

            static Task EnqueueReturnedErrorMessage(
                (AsyncEventingBasicConsumer, BasicReturnEventArgs, IReadOnlyDictionary<string, EndpointIdentity>, IJsonSerializer, Func<EndpointIdentity, IntegrationMessage, Exception, CancellationToken, Task>) state,
                CancellationToken token)
            {
                var (consumer, args, consumers, jsonSerializer, enqueueError) = state;

                var endpointIdentity = GetEndpointIdentity(consumer, consumers);

                var message = args.DecodeIntegrationMessage(jsonSerializer);

                var exception = new InvalidOperationException($"{args.ReplyCode}: {args.ReplyText}");

                return enqueueError(endpointIdentity, message, exception, token);
            }

            static EndpointIdentity GetEndpointIdentity(
                AsyncEventingBasicConsumer consumer,
                IReadOnlyDictionary<string, EndpointIdentity> consumers)
            {
                var consumerTag = consumer.ConsumerTags.Single();

                if (!consumers.TryGetValue(consumerTag, out var endpointIdentity))
                {
                    throw new InvalidOperationException($"Unable to find appropriate endpoint for consumer with tag: {consumerTag}");
                }

                return endpointIdentity;
            }
        }

        private static AsyncEventHandler<BasicDeliverEventArgs> HandleReceivedMessage(
            ILogger logger,
            IReadOnlyDictionary<string, EndpointIdentity> consumers,
            IReadOnlyDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>> messageHandlers,
            IJsonSerializer jsonSerializer,
            AsyncManualResetEvent ready,
            CancellationToken token)
        {
            return async (sender, args) =>
            {
                await ready.WaitAsync(token).ConfigureAwait(false);

                var consumer = (AsyncEventingBasicConsumer)sender;

                await ExecutionExtensions
                   .TryAsync((consumer, args, consumers, messageHandlers, jsonSerializer), InvokeMessageHandlers)
                   .Catch<Exception>((exception, _) =>
                   {
                       logger.Error(exception);
                       args.Nack(consumer.Model);
                       return Task.CompletedTask;
                   })
                   .Invoke(token)
                   .ConfigureAwait(false);
            };

            static async Task InvokeMessageHandlers(
                (AsyncEventingBasicConsumer, BasicDeliverEventArgs, IReadOnlyDictionary<string, EndpointIdentity>, IReadOnlyDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>>, IJsonSerializer) state,
                CancellationToken token)
            {
                var (consumer, args, consumers, messageHandlers, jsonSerializer) = state;

                var message = args.DecodeIntegrationMessage(jsonSerializer);

                ManageMessageHeaders(message, args);

                await GetMessageHandler(consumer, consumers, messageHandlers)
                   .Invoke(message, token)
                   .ConfigureAwait(false);
            }

            static void ManageMessageHeaders(
                IntegrationMessage integrationMessage,
                BasicDeliverEventArgs args)
            {
                integrationMessage.OverwriteHeader(new ActualDeliveryDate(DateTime.UtcNow));
                integrationMessage.OverwriteHeader(new DeliveryTag(args.DeliveryTag));
            }

            static Func<IntegrationMessage, CancellationToken, Task> GetMessageHandler(
                AsyncEventingBasicConsumer consumer,
                IReadOnlyDictionary<string, EndpointIdentity> consumers,
                IReadOnlyDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>> messageHandlers)
            {
                if (consumer.ConsumerTags.Length != 1)
                {
                    throw new InvalidOperationException($"Consumer has several consumer tags: {string.Join(", ", consumer.ConsumerTags)}");
                }

                var consumerTag = consumer.ConsumerTags.Single();

                if (!consumers.TryGetValue(consumerTag, out var endpointIdentity))
                {
                    throw new InvalidOperationException($"Unable to find appropriate endpoint for consumer with tag: {consumerTag}");
                }

                if (!messageHandlers.TryGetValue(endpointIdentity, out var messageHandler))
                {
                    throw new InvalidOperationException($"Unable to find appropriate message handler for endpoint: {endpointIdentity}");
                }

                return messageHandler;
            }
        }

        #endregion
    }
}