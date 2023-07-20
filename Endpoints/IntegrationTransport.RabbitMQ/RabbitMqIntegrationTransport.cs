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
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Logging;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;
    using Microsoft.Extensions.Logging;
    using Settings;

    [SuppressMessage("Analysis", "CA1506", Justification = "Infrastructural code")]
    [Component(EnLifestyle.Singleton)]
    internal class RabbitMqIntegrationTransport : IIntegrationTransport,
                                                  IConfigurableIntegrationTransport,
                                                  IExecutableIntegrationTransport,
                                                  IResolvable<IIntegrationTransport>,
                                                  IResolvable<IConfigurableIntegrationTransport>,
                                                  IResolvable<IExecutableIntegrationTransport>,
                                                  IDisposable
    {
        public const string ContentEncoding = "gzip";

        private const string InputExchange = nameof(InputExchange);

        private const string DeadLetterExchange = nameof(DeadLetterExchange);
        private const string DeadLetterQueue = nameof(DeadLetterQueue);

        private const string DeferredExchange = nameof(DeferredExchange);

        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        private readonly RabbitMqSettings _rabbitMqSettings;

        private readonly ConcurrentDictionary<EndpointIdentity, object?> _endpoints;
        private readonly ConcurrentDictionary<EndpointIdentity, IIntegrationTypeProvider> _integrationMessageTypes;

        private readonly ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>> _messageHandlers;
        private readonly ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>> _errorMessageHandlers;

        private readonly ConcurrentDictionary<EndpointIdentity, IModel> _channels;
        private readonly ConcurrentDictionary<string, EndpointIdentity> _consumers;
        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> _outstandingConfirms;

        private readonly AsyncManualResetEvent _ready;
        private readonly AsyncAutoResetEvent _sync;

        private readonly TaskCompletionSource<object?> _backgroundMessageProcessingTcs;

        private readonly EventHandler<ShutdownEventArgs> _handleConnectionShutdownSubscription;

        private readonly EventHandler<ShutdownEventArgs> _handleChannelShutdownSubscription;
        private readonly EventHandler<CallbackExceptionEventArgs> _handleChannelCallbackExceptionSubscription;
        private readonly EventHandler<BasicReturnEventArgs> _handleChannelBasicReturnSubscription;
        private readonly EventHandler<BasicAckEventArgs> _handleChannelBasicAcksSubscription;
        private readonly EventHandler<BasicNackEventArgs> _handleChannelBasicNacksSubscription;

        private readonly AsyncEventHandler<BasicDeliverEventArgs> _handleReceivedMessageSubscription;
        private readonly AsyncEventHandler<ShutdownEventArgs> _handleConsumerShutdownSubscription;

        private EnIntegrationTransportStatus _status;

        private IConnection? _connection;

        [SuppressMessage("Analysis", "CA2213", Justification = "singleton instance keeps reference forever")]
        private CancellationTokenSource? _cts;

        public RabbitMqIntegrationTransport(
            ILogger logger,
            IJsonSerializer jsonSerializer,
            ISettingsProvider<RabbitMqSettings> rabbitMqSettingsProvider)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;

            _rabbitMqSettings = rabbitMqSettingsProvider.Get();

            _endpoints = new ConcurrentDictionary<EndpointIdentity, object?>();
            _integrationMessageTypes = new ConcurrentDictionary<EndpointIdentity, IIntegrationTypeProvider>();

            _messageHandlers = new ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>();
            _errorMessageHandlers = new ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>>();

            _channels = new ConcurrentDictionary<EndpointIdentity, IModel>();
            _consumers = new ConcurrentDictionary<string, EndpointIdentity>(StringComparer.Ordinal);
            _outstandingConfirms = new ConcurrentDictionary<ulong, TaskCompletionSource<bool>>();

            _ready = new AsyncManualResetEvent(false);
            _sync = new AsyncAutoResetEvent(true);

            _backgroundMessageProcessingTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            _handleConnectionShutdownSubscription = HandleConnectionShutdown(_logger, _backgroundMessageProcessingTcs);

            _handleChannelShutdownSubscription = HandleChannelShutdown(_logger, _backgroundMessageProcessingTcs);
            _handleChannelCallbackExceptionSubscription = HandleChannelCallbackException(_logger, _backgroundMessageProcessingTcs);
            _handleChannelBasicReturnSubscription = HandleChannelBasicReturn(_logger, OnMessageReceived, _jsonSerializer, _ready, GetCancellationToken);
            _handleChannelBasicAcksSubscription = HandleChannelBasicAcks();
            _handleChannelBasicNacksSubscription = HandleChannelBasicNacks();

            _handleReceivedMessageSubscription = HandleReceivedMessage(_logger, _consumers, _messageHandlers, OnMessageReceived, _jsonSerializer, _ready, GetCancellationToken);
            _handleConsumerShutdownSubscription = HandleConsumerShutdown(_logger, _backgroundMessageProcessingTcs);

            _status = EnIntegrationTransportStatus.Stopped;
        }

        public event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        public event EventHandler<IntegrationTransportMessageReceivedEventArgs>? MessageReceived;

        public EnIntegrationTransportStatus Status
        {
            get => _status;

            private set
            {
                var previousValue = _status;

                _status = value;

                StatusChanged?.Invoke(this, new IntegrationTransportStatusChangedEventArgs(previousValue, value));
            }
        }

        private CancellationToken Token => GetCancellationToken();

        public void Dispose()
        {
            _ready.Reset();

            foreach (var (_, channel) in _channels)
            {
                channel.ModelShutdown -= _handleChannelShutdownSubscription;
                channel.CallbackException -= _handleChannelCallbackExceptionSubscription;
                channel.BasicReturn -= _handleChannelBasicReturnSubscription;
                channel.BasicAcks -= _handleChannelBasicAcksSubscription;
                channel.BasicNacks -= _handleChannelBasicNacksSubscription;

                channel.Close();
                channel.Dispose();
            }

            _channels.Clear();

            var connection = Interlocked.Exchange(ref _connection, null);

            if (connection != null)
            {
                connection.ConnectionShutdown -= _handleConnectionShutdownSubscription;

                connection.Close();
                connection.Dispose();
            }

            if (Status != EnIntegrationTransportStatus.Stopped)
            {
                Status = EnIntegrationTransportStatus.Stopped;
            }
        }

        public void Bind(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
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

            var result = true;

            var messageTypes = message
               .ReflectedType
               .IncludedTypes()
               .Where(type => typeof(IIntegrationMessage).IsAssignableFrom(type)
                           && !type.IsMessageContractAbstraction())
               .ToList();

            var channel = _channels[message.ReadRequiredHeader<SentFrom>().Value];

            foreach (var type in messageTypes)
            {
                var copy = message.ContravariantClone(type);
                result = await Publish(copy, channel, _rabbitMqSettings, _outstandingConfirms, _jsonSerializer).ConfigureAwait(false);
            }

            return result;

            static Task<bool> Publish(
                IntegrationMessage message,
                IModel channel,
                RabbitMqSettings rabbitMqSettings,
                ConcurrentDictionary<ulong, TaskCompletionSource<bool>> outstandingConfirms,
                IJsonSerializer jsonSerializer)
            {
                var basicProperties = CreateBasicProperties(channel, message, rabbitMqSettings);

                var exchange = basicProperties.Expiration.IsNullOrEmpty()
                    ? InputExchange
                    : (string)basicProperties.Headers[DeferredExchange];

                var bodyBytes = message.EncodeIntegrationMessage(jsonSerializer);

                var left = message.ReflectedType.GenericTypeDefinitionOrSelf().FullName!.GetRoutingKeyPart();

                var right = message.GetTargetEndpoint();

                var routingKey = string.Join(".", left, right);

                return channel.Publish(exchange, routingKey, basicProperties, bodyBytes, outstandingConfirms);
            }

            static IBasicProperties CreateBasicProperties(
                IModel channel,
                IntegrationMessage message,
                RabbitMqSettings rabbitMqSettings)
            {
                var basicProperties = channel.CreateBasicProperties();

                basicProperties.DeliveryMode = 2;

                basicProperties.ContentType = MediaTypeNames.Application.Json;
                basicProperties.ContentEncoding = ContentEncoding;

                basicProperties.AppId = message.ReadRequiredHeader<SentFrom>().Value.LogicalName;
                basicProperties.ClusterId = message.ReadRequiredHeader<SentFrom>().Value.InstanceName;
                basicProperties.MessageId = message.ReadRequiredHeader<Id>().Value.ToString();
                basicProperties.CorrelationId = message.ReadRequiredHeader<ConversationId>().Value.ToString();
                basicProperties.Type = message.ReflectedType.GenericTypeDefinitionOrSelf().FullName;
                basicProperties.Headers = new Dictionary<string, object>();

                var deferredUntil = message.ReadHeader<DeferredUntil>();
                var now = DateTime.UtcNow;
                var expirationMilliseconds = deferredUntil != null && deferredUntil.Value >= now
                    ? (int)Math.Round((deferredUntil.Value - now).TotalMilliseconds, MidpointRounding.AwayFromZero)
                    : 0;

                var isDeferred = expirationMilliseconds > 100;

                if (isDeferred)
                {
                    basicProperties.Expiration = expirationMilliseconds.ToString(CultureInfo.InvariantCulture);
                    var deferredExchange = BuildDeferredPath(channel, expirationMilliseconds, rabbitMqSettings);
                    basicProperties.Headers[DeferredExchange] = deferredExchange;
                }

                return basicProperties;
            }

            static string BuildDeferredPath(
                IModel channel,
                int expirationMilliseconds,
                RabbitMqSettings rabbitMqSettings)
            {
                var generatedName = Guid.NewGuid().ToString();

                channel.DeclareExchange(
                    generatedName,
                    ExchangeType.Fanout,
                    autoDelete: true);

                channel.DeclareQueue(
                    generatedName,
                    new Dictionary<string, object>
                    {
                        ["x-queue-type"] = "quorum",
                        ["x-max-length-bytes"] = rabbitMqSettings.QueueMaxLengthBytes,
                        ["x-dead-letter-exchange"] = InputExchange,
                        ["x-dead-letter-strategy"] = "at-least-once",
                        ["x-overflow"] = "reject-publish",
                        ["x-expires"] = expirationMilliseconds + 4200
                    });

                channel.BindQueue(
                    generatedName,
                    generatedName,
                    string.Empty);

                return generatedName;
            }
        }

        public Task StartBackgroundMessageProcessing(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            _cts.Token.Register(() => _backgroundMessageProcessingTcs.TrySetCanceled());

            ConfigureErrorHandlers(_channels, _endpoints.Keys, BindErrorHandler);

            return RestartBackgroundMessageProcessing(_backgroundMessageProcessingTcs, Token);
        }

        private CancellationToken GetCancellationToken()
        {
            return _cts?.Token
                ?? throw new InvalidOperationException($"Call {nameof(RabbitMqIntegrationTransport)}.{nameof(StartBackgroundMessageProcessing)} before");
        }

        private async Task RestartBackgroundMessageProcessing(
            TaskCompletionSource<object?> tcs,
            CancellationToken token)
        {
            try
            {
                await _sync.WaitAsync(token).ConfigureAwait(false);
                await RestartBackgroundMessageProcessingUnsafe(tcs, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _sync.Set();
            }
        }

        private async Task RestartBackgroundMessageProcessingUnsafe(
            TaskCompletionSource<object?> tcs,
            CancellationToken token)
        {
            Dispose();

            Status = EnIntegrationTransportStatus.Starting;

            await _rabbitMqSettings
               .DeclareVirtualHost(_jsonSerializer, _logger, token)
               .ConfigureAwait(false);

            _connection = await ConfigureConnection(
                    _logger,
                    _rabbitMqSettings,
                    (ushort)_endpoints.Keys.Count,
                    _handleConnectionShutdownSubscription,
                    token)
               .ConfigureAwait(false);

            using (var commonChannel = ConfigureChannel(
                       _connection,
                       _rabbitMqSettings,
                       (_, _) => { },
                       (_, _) => { },
                       (_, _) => { },
                       (_, _) => { },
                       (_, _) => { }))
            {
                BuildTopology(commonChannel,
                    _rabbitMqSettings,
                    _endpoints.Keys,
                    _integrationMessageTypes);

                commonChannel.Close();
            }

            ConfigureChannels(
                _connection,
                _channels,
                _endpoints.Keys,
                _rabbitMqSettings,
                _handleChannelShutdownSubscription,
                _handleChannelCallbackExceptionSubscription,
                _handleChannelBasicReturnSubscription,
                _handleChannelBasicAcksSubscription,
                _handleChannelBasicNacksSubscription);

            StartConsumers(
                _rabbitMqSettings,
                _channels,
                _consumers,
                _handleReceivedMessageSubscription,
                _handleConsumerShutdownSubscription);

            Status = EnIntegrationTransportStatus.Running;

            _ready.Set();

            await tcs.Task.ConfigureAwait(false);
        }

        #region configuration

        private static async Task<IConnection> ConfigureConnection(
            ILogger logger,
            RabbitMqSettings rabbitMqSettings,
            ushort channelsCount,
            EventHandler<ShutdownEventArgs> handleConnectionShutdownSubscription,
            CancellationToken token)
        {
            for (var i = 0; i < 4; i++)
            {
                logger.Information($"Trying to establish connection with RabbitMQ broker: {i}");

                try
                {
                    var connectionFactory = new ConnectionFactory
                    {
                        Port = rabbitMqSettings.Port,
                        UserName = rabbitMqSettings.User,
                        Password = rabbitMqSettings.Password,
                        VirtualHost = rabbitMqSettings.VirtualHost,
                        ClientProvidedName = rabbitMqSettings.ApplicationName,
                        DispatchConsumersAsync = true,
                        AutomaticRecoveryEnabled = true,
                        TopologyRecoveryEnabled = false,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                        RequestedChannelMax = channelsCount
                    };

                    var endpoints = rabbitMqSettings
                       .Hosts
                       .Select(host => new AmqpTcpEndpoint(host))
                       .ToList();

                    var connection = connectionFactory.CreateConnection(endpoints);

                    connection.ConnectionShutdown += handleConnectionShutdownSubscription;

                    logger.Information("Connection with RabbitMQ broker was successfully established");

                    return connection;
                }
                catch (BrokerUnreachableException brokerUnreachableException)
                {
                    logger.Error(brokerUnreachableException, $"{nameof(RabbitMqIntegrationTransport)}.{nameof(ConfigureConnection)}");
                }

                try
                {
                    await Task
                       .Delay(TimeSpan.FromSeconds(15), token)
                       .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            throw new BrokerUnreachableException(new InvalidOperationException("Unable to establish connection with RabbitMQ message broker"));
        }

        private static void ConfigureChannels(
            IConnection connection,
            ConcurrentDictionary<EndpointIdentity, IModel> channels,
            IEnumerable<EndpointIdentity> endpoints,
            RabbitMqSettings rabbitMqSettings,
            EventHandler<ShutdownEventArgs> handleChannelShutdownSubscription,
            EventHandler<CallbackExceptionEventArgs> handleChannelCallbackExceptionSubscription,
            EventHandler<BasicReturnEventArgs> handleChannelBasicReturnSubscription,
            EventHandler<BasicAckEventArgs> handleChannelBasicAcksSubscription,
            EventHandler<BasicNackEventArgs> handleChannelBasicNacksSubscription)
        {
            foreach (var endpointIdentity in endpoints)
            {
                var channel = ConfigureChannel(
                    connection,
                    rabbitMqSettings,
                    handleChannelShutdownSubscription,
                    handleChannelCallbackExceptionSubscription,
                    handleChannelBasicReturnSubscription,
                    handleChannelBasicAcksSubscription,
                    handleChannelBasicNacksSubscription);

                channels.AddOrUpdate(endpointIdentity, _ => channel, (_, _) => channel);
            }
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
            channel.BasicQos(0, rabbitMqSettings.ConsumerPrefetchCount, false);

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
            BuildInputPath(channel, rabbitMqSettings, endpoints, integrationMessageTypes);

            static void BuildDeadLetterPath(IModel channel, RabbitMqSettings rabbitMqSettings)
            {
                channel.DeclareExchange(
                    DeadLetterExchange,
                    ExchangeType.Fanout);

                channel.DeclareQueue(
                    DeadLetterQueue,
                    new Dictionary<string, object>
                    {
                        ["x-queue-type"] = "quorum",
                        ["x-max-length-bytes"] = rabbitMqSettings.QueueMaxLengthBytes,
                        ["x-overflow"] = "reject-publish"
                    });

                channel.BindQueue(
                    DeadLetterQueue,
                    DeadLetterExchange,
                    string.Empty);
            }

            static void BuildInputPath(
                IModel channel,
                RabbitMqSettings rabbitMqSettings,
                ICollection<EndpointIdentity> endpoints,
                IReadOnlyDictionary<EndpointIdentity, IIntegrationTypeProvider> integrationMessageTypes)
            {
                channel.DeclareExchange(
                    InputExchange,
                    ExchangeType.Topic);

                foreach (var endpointIdentity in endpoints)
                {
                    channel.DeclareExchange(
                        endpointIdentity.LogicalName,
                        ExchangeType.Fanout);

                    channel.DeclareQueue(
                        endpointIdentity.LogicalName,
                        new Dictionary<string, object>
                        {
                            ["x-queue-type"] = "quorum",
                            ["x-max-length-bytes"] = rabbitMqSettings.QueueMaxLengthBytes,
                            ["x-dead-letter-exchange"] = DeadLetterExchange,
                            ["x-dead-letter-strategy"] = "at-least-once",
                            ["x-overflow"] = "reject-publish"
                        });

                    channel.BindQueue(
                        endpointIdentity.LogicalName,
                        endpointIdentity.LogicalName,
                        string.Empty);
                }

                foreach (var (endpointIdentity, provider) in integrationMessageTypes)
                {
                    BuildLeftInputPath(channel, provider.EndpointCommands());
                    BuildLeftInputPath(channel, provider.EventsSubscriptions());
                    BuildLeftInputPath(channel, provider.EndpointRequests());
                    BuildLeftInputPath(channel, provider.RepliesSubscriptions());

                    BuildRightInputPath(channel, endpointIdentity, provider.EndpointCommands());
                    BuildRightInputPathForEvents(channel, endpointIdentity, provider.EventsSubscriptions());
                    BuildRightInputPath(channel, endpointIdentity, provider.EndpointRequests());
                    BuildRightInputPath(channel, endpointIdentity, provider.RepliesSubscriptions());
                }

                static void BuildLeftInputPath(
                    IModel channel,
                    IReadOnlyCollection<Type> messageTypes)
                {
                    foreach (var messageType in messageTypes)
                    {
                        channel.DeclareExchange(
                            messageType.FullName!,
                            ExchangeType.Topic);

                        channel.BindExchange(
                            InputExchange,
                            messageType.FullName!,
                            $"{messageType.FullName!.GetRoutingKeyPart()}.*");
                    }
                }

                static void BuildRightInputPath(
                    IModel channel,
                    EndpointIdentity endpointIdentity,
                    IReadOnlyCollection<Type> messageTypes)
                {
                    foreach (var messageType in messageTypes)
                    {
                        channel.BindExchange(
                            messageType.FullName!,
                            endpointIdentity.LogicalName,
                            $"*.{endpointIdentity.LogicalName}");
                    }
                }

                static void BuildRightInputPathForEvents(
                    IModel channel,
                    EndpointIdentity endpointIdentity,
                    IReadOnlyCollection<Type> messageTypes)
                {
                    foreach (var messageType in messageTypes)
                    {
                        channel.BindExchange(
                            messageType.FullName!,
                            endpointIdentity.LogicalName,
                            "*.*");
                    }
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
            RabbitMqSettings rabbitMqSettings,
            IReadOnlyDictionary<EndpointIdentity, IModel> channels,
            ConcurrentDictionary<string, EndpointIdentity> consumers,
            AsyncEventHandler<BasicDeliverEventArgs> handleReceivedMessageSubscription,
            AsyncEventHandler<ShutdownEventArgs> handleConsumerShutdownSubscription)
        {
            foreach (var (endpointIdentity, channel) in channels)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);

                var args = new Dictionary<string, object>
                {
                    ["x-priority"] = (int)rabbitMqSettings.ConsumerPriority
                };

                consumer.Received += handleReceivedMessageSubscription;
                consumer.Shutdown += handleConsumerShutdownSubscription;

                var consumerTag = channel.BasicConsume(
                    queue: endpointIdentity.LogicalName,
                    autoAck: false,
                    arguments: args,
                    consumer: consumer);

                consumers.Add(consumerTag, endpointIdentity);
            }
        }

        #endregion

        #region error_event_handlers

        private EventHandler<ShutdownEventArgs> HandleConnectionShutdown(
            ILogger logger,
            TaskCompletionSource<object?> tcs)
        {
            return (_, args) =>
            {
                Shutdown(logger, tcs, new InvalidOperationException(args.ToString()), nameof(HandleConnectionShutdown));
            };
        }

        private EventHandler<ShutdownEventArgs> HandleChannelShutdown(
            ILogger logger,
            TaskCompletionSource<object?> tcs)
        {
            return (_, args) =>
            {
                Shutdown(logger, tcs, new InvalidOperationException(args.ToString()), nameof(HandleChannelShutdown));
            };
        }

        private AsyncEventHandler<ShutdownEventArgs> HandleConsumerShutdown(
            ILogger logger,
            TaskCompletionSource<object?> tcs)
        {
            return (_, args) =>
            {
                Shutdown(logger, tcs, new InvalidOperationException(args.ToString()), nameof(HandleConsumerShutdown));
                return Task.CompletedTask;
            };
        }

        private EventHandler<CallbackExceptionEventArgs> HandleChannelCallbackException(
            ILogger logger,
            TaskCompletionSource<object?> tcs)
        {
            return (_, args) =>
            {
                Shutdown(logger, tcs, new InvalidOperationException(ShowDetails(args.Detail), args.Exception), nameof(HandleChannelCallbackException));

                static string ShowDetails(IDictionary<string, object> dict)
                {
                    if ((dict.TryGetValue("consumer", out var value) || dict.TryGetValue("context", out value))
                        && value is AsyncEventingBasicConsumer consumer
                        && !consumer.IsRunning)
                    {
                        return $"{nameof(AsyncEventingBasicConsumer.ShutdownReason)}: {consumer.ShutdownReason}";
                    }

                    return nameof(HandleChannelCallbackException);
                }
            };
        }

        private void Shutdown(
            ILogger logger,
            TaskCompletionSource<object?> tcs,
            Exception exception,
            string message)
        {
            logger.Error(exception, $"{nameof(RabbitMqIntegrationTransport)}.{message}");

            RestartBackgroundMessageProcessing(tcs, Token).Wait(Token);
        }

        #endregion

        #region publisher_confirms

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
                _ = tcs.TrySetResult(result);
            }
        }

        #endregion

        #region delivery_event_handlers

        private static EventHandler<BasicReturnEventArgs> HandleChannelBasicReturn(
            ILogger logger,
            Action<Func<IntegrationMessage>, Exception?> onMessageReceived,
            IJsonSerializer jsonSerializer,
            AsyncManualResetEvent ready,
            Func<CancellationToken> token)
        {
            return (sender, args) =>
            {
                ready.WaitAsync(token()).Wait(token());

                new Action(() => EnqueueReturnedErrorMessage((IModel)sender, args, onMessageReceived, jsonSerializer, logger))
                   .Try()
                   .Catch<Exception>(exception => logger.Error(exception, $"{nameof(RabbitMqIntegrationTransport)}.{nameof(HandleChannelBasicReturn)} - {args.BasicProperties.MessageId}"))
                   .Invoke();
            };

            static void EnqueueReturnedErrorMessage(
                IModel recoveryAwareModel,
                BasicReturnEventArgs args,
                Action<Func<IntegrationMessage>, Exception?> onMessageReceived,
                IJsonSerializer jsonSerializer,
                ILogger logger)
            {
                var exception = new InvalidOperationException($"{args.ReplyCode}: {args.ReplyText} - {args.BasicProperties.Type}: {args.BasicProperties.MessageId}");

                logger.Error(exception, $"{nameof(RabbitMqIntegrationTransport)}.{nameof(HandleChannelBasicReturn)} - {args.BasicProperties.MessageId}");

                onMessageReceived(() => args.DecodeIntegrationMessage(jsonSerializer), exception);

                recoveryAwareModel.Publish(DeadLetterExchange, args.RoutingKey, args.BasicProperties, args.Body.ToArray());
            }
        }

        private void OnMessageReceived(Func<IntegrationMessage> messageProducer, Exception? exception)
        {
            MessageReceived?.Invoke(this, new IntegrationTransportMessageReceivedEventArgs(messageProducer(), exception));
        }

        [SuppressMessage("Analysis", "CA1031", Justification = "async event handler with void as retun value")]
        private AsyncEventHandler<BasicDeliverEventArgs> HandleReceivedMessage(
            ILogger logger,
            IReadOnlyDictionary<string, EndpointIdentity> consumers,
            IReadOnlyDictionary<EndpointIdentity, Func<IntegrationMessage, Task>> messageHandlers,
            Action<Func<IntegrationMessage>, Exception?> onMessageReceived,
            IJsonSerializer jsonSerializer,
            AsyncManualResetEvent ready,
            Func<CancellationToken> token)
        {
            return async (sender, args) =>
            {
                await ready.WaitAsync(token()).ConfigureAwait(false);

                var consumer = (AsyncEventingBasicConsumer)sender;

                try
                {
                    var endpointIdentity = GetEndpointIdentityForConsumer(consumer, consumers);

                    var message = args.DecodeIntegrationMessage(jsonSerializer);

                    ManageMessageHeaders(message, args);

                    await InvokeMessageHandler(consumer, args, endpointIdentity, message, messageHandlers, onMessageReceived)
                        .TryAsync()
                        .Catch<Exception>((exception, t) => EnqueueError(endpointIdentity, message, exception, t))
                        .Invoke(token())
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    logger.Error(exception, $"{nameof(RabbitMqIntegrationTransport)}.{nameof(HandleReceivedMessage)} - {args.BasicProperties.MessageId}");
                    args.Nack(consumer.Model);
                }
            };

            static async Task InvokeMessageHandler(
                AsyncEventingBasicConsumer consumer,
                BasicDeliverEventArgs args,
                EndpointIdentity endpointIdentity,
                IntegrationMessage message,
                IReadOnlyDictionary<EndpointIdentity, Func<IntegrationMessage, Task>> messageHandlers,
                Action<Func<IntegrationMessage>, Exception?> onMessageReceived)
            {
                onMessageReceived(() => message, default);

                try
                {
                    await GetMessageHandler(endpointIdentity, messageHandlers)
                       .Invoke(message)
                       .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var rejectReason = message.ReadHeader<RejectReason>();

                if (rejectReason?.Value != null)
                {
                    throw rejectReason.Value.Rethrow();
                }

                args.Ack(consumer.Model);
            }

            static void ManageMessageHeaders(
                IntegrationMessage integrationMessage,
                BasicDeliverEventArgs args)
            {
                integrationMessage.OverwriteHeader(new ActualDeliveryDate(DateTime.UtcNow));
                integrationMessage.OverwriteHeader(new DeliveryTag(args.DeliveryTag));
            }

            static EndpointIdentity GetEndpointIdentityForConsumer(
                AsyncEventingBasicConsumer consumer,
                IReadOnlyDictionary<string, EndpointIdentity> consumers)
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

                return endpointIdentity;
            }

            static Func<IntegrationMessage, Task> GetMessageHandler(
                EndpointIdentity endpointIdentity,
                IReadOnlyDictionary<EndpointIdentity, Func<IntegrationMessage, Task>> messageHandlers)
            {
                if (!messageHandlers.TryGetValue(endpointIdentity, out var messageHandler))
                {
                    throw new InvalidOperationException($"Unable to find appropriate message handler for endpoint: {endpointIdentity}");
                }

                return messageHandler;
            }
        }

        private Task EnqueueError(
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token)
        {
            OnMessageReceived(() => message, exception);

            if (_errorMessageHandlers.TryGetValue(endpointIdentity, out var handlers))
            {
                return Task.WhenAll(handlers.Select(handler => handler(message, exception, token)));
            }

            _logger.Error(exception, $"Message handling error: {message.ReflectedType.FullName}");

            return Task.CompletedTask;
        }

        #endregion
    }
}