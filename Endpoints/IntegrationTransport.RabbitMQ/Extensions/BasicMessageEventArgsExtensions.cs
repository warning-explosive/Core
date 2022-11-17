namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using Basics;
    using CrossCuttingConcerns.Json;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;

    internal static class BasicMessageEventArgsExtensions
    {
        public static IntegrationMessage DecodeIntegrationMessage(
            this BasicDeliverEventArgs args,
            IJsonSerializer jsonSerializer)
        {
            return DecodeIntegrationMessage(args.BasicProperties, args.Body, jsonSerializer);
        }

        public static IntegrationMessage DecodeIntegrationMessage(
            this BasicReturnEventArgs args,
            IJsonSerializer jsonSerializer)
        {
            return DecodeIntegrationMessage(args.BasicProperties, args.Body, jsonSerializer);
        }

        public static Memory<byte> EncodeIntegrationMessage(
            this IntegrationMessage message,
            IJsonSerializer jsonSerializer)
        {
            var serializedMessage = jsonSerializer.SerializeObject(message);
            ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(serializedMessage);
            return bytes.Compress();
        }

        public static void Publish(
            this IModel channel,
            string exchange,
            string routingKey,
            IBasicProperties basicProperties,
            byte[] body)
        {
            lock (channel)
            {
                channel.BasicPublish(exchange, routingKey, true, basicProperties, body);
            }
        }

        public static Task<bool> Publish(
            this IModel channel,
            string exchange,
            string routingKey,
            IBasicProperties basicProperties,
            ReadOnlyMemory<byte> bodyBytes,
            ConcurrentDictionary<ulong, TaskCompletionSource<bool>> outstandingConfirms)
        {
            Task<bool> confirmedPublication;

            lock (channel)
            {
                confirmedPublication = outstandingConfirms
                   .GetOrAdd(channel.NextPublishSeqNo, _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously))
                   .Task;

                channel.BasicPublish(exchange, routingKey, true, basicProperties, bodyBytes);
            }

            return confirmedPublication;
        }

        public static void Ack(
            this BasicDeliverEventArgs args,
            IModel channel)
        {
            lock (channel)
            {
                channel.BasicAck(args.DeliveryTag, false);
            }
        }

        public static void Nack(
            this BasicDeliverEventArgs args,
            IModel channel)
        {
            lock (channel)
            {
                channel.BasicNack(args.DeliveryTag, false, false);
            }
        }

        public static void Nack(
            this IntegrationMessage integrationMessage,
            IModel channel)
        {
            var deliveryTag = integrationMessage.ReadRequiredHeader<DeliveryTag>().Value;

            lock (channel)
            {
                channel.BasicNack(deliveryTag, false, false);
            }
        }

        private static IntegrationMessage DecodeIntegrationMessage(
            this IBasicProperties basicProperties,
            ReadOnlyMemory<byte> body,
            IJsonSerializer jsonSerializer)
        {
            if (!basicProperties.ContentEncoding.Equals(RabbitMqIntegrationTransport.ContentEncoding, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"Unknown {nameof(basicProperties.ContentEncoding)}.{basicProperties.ContentEncoding}. Supported {nameof(basicProperties.ContentEncoding)}: {RabbitMqIntegrationTransport.ContentEncoding}.");
            }

            if (!basicProperties.ContentType.Equals(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"Unknown {nameof(basicProperties.ContentType)}.{basicProperties.ContentType}. Supported {nameof(basicProperties.ContentType)}: {MediaTypeNames.Application.Json}.");
            }

            ReadOnlySpan<byte> bytes = body.Span.Decompress().Span;
            var serializedMessage = Encoding.UTF8.GetString(bytes);
            return jsonSerializer.DeserializeObject<IntegrationMessage>(serializedMessage);
        }
    }
}