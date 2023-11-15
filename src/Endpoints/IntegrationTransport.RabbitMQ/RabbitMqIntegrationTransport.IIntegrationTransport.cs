namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Json;
    using Extensions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using global::RabbitMQ.Client;
    using Settings;

    /// <summary>
    /// IIntegrationTransport
    /// </summary>
    internal partial class RabbitMqIntegrationTransport
    {
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

        public async Task<bool> Enqueue(IntegrationMessage message, CancellationToken token)
        {
            await _ready
                .WaitAsync(token)
                .ConfigureAwait(false);

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

                var routingKey = (left, right).ToString(".");

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
    }
}