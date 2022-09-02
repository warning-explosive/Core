namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossCuttingConcerns.Json;
    using global::RabbitMQ.Client;
    using Microsoft.Extensions.Logging;
    using Settings;

    internal static class DeclarationExtensions
    {
        public static async Task DeclareVirtualHost(
            this RabbitMqSettings rabbitMqSettings,
            IJsonSerializer jsonSerializer,
            ILogger logger,
            CancellationToken token)
        {
            var virtualHosts = (await rabbitMqSettings
                   .ReadVirtualHosts(jsonSerializer, logger, token)
                   .ConfigureAwait(false))
               .Select(virtualHost => virtualHost.Name)
               .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!virtualHosts.Contains(rabbitMqSettings.VirtualHost))
            {
                await rabbitMqSettings
                   .CreateVirtualHost(logger, token)
                   .ConfigureAwait(false);
            }
        }

        public static string GetRoutingKeyPart(this string str)
        {
            return str
               .Replace(".", string.Empty, StringComparison.OrdinalIgnoreCase)
               .Replace("+", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public static void DeclareExchange(
            this IModel channel,
            string exchange,
            string type,
            bool durable = true,
            bool autoDelete = false)
        {
            channel.ExchangeDeclare(
                exchange,
                type,
                durable,
                autoDelete,
                new Dictionary<string, object>());
        }

        public static void DeclareQueue(
            this IModel channel,
            string queue,
            IDictionary<string, object> arguments,
            bool durable = true,
            bool exclusive = false,
            bool autoDelete = false)
        {
            channel.QueueDeclare(queue,
                durable,
                exclusive,
                autoDelete,
                arguments);
        }

        public static void BindExchange(
            this IModel channel,
            string source,
            string destination,
            string routingKey)
        {
            channel.ExchangeBind(
                destination,
                source,
                routingKey,
                new Dictionary<string, object>());
        }

        public static void BindQueue(
            this IModel channel,
            string queue,
            string exchange,
            string routingKey)
        {
            channel.QueueBind(
                queue,
                exchange,
                routingKey,
                new Dictionary<string, object>());
        }

        public static void BindQueue(
            this IModel channel,
            string queue,
            IEnumerable<string> exchanges,
            string routingKey)
        {
            foreach (var exchange in exchanges)
            {
                BindQueue(channel, queue, exchange, routingKey);
            }
        }
    }
}