namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossCuttingConcerns.Json;
    using global::RabbitMQ.Client;
    using Settings;

    internal static class DeclarationExtensions
    {
        public static async Task DeclareVirtualHost(
            this RabbitMqSettings rabbitMqSettings,
            IJsonSerializer jsonSerializer,
            CancellationToken token)
        {
            var virtualHosts = (await rabbitMqSettings
                   .ReadVirtualHosts(jsonSerializer, token)
                   .ConfigureAwait(false))
               .Select(virtualHost => virtualHost.Name)
               .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!virtualHosts.Contains(rabbitMqSettings.VirtualHost))
            {
                await rabbitMqSettings
                   .CreateVirtualHost(token)
                   .ConfigureAwait(false);
            }
        }

        public static void DeclareExchange(
            this IModel channel,
            string exchange,
            string type)
        {
            channel.ExchangeDeclare(
                exchange,
                type,
                true,
                false,
                new Dictionary<string, object>());
        }

        public static void DeclareQueue(
            this IModel channel,
            string queue,
            IDictionary<string, object> arguments)
        {
            channel.QueueDeclare(queue,
                true,
                false,
                false,
                arguments);
        }

        public static void BindExchange(
            this IModel channel,
            string source,
            string destination,
            string routingKey)
        {
            channel.ExchangeBind(
                source,
                destination,
                routingKey,
                new Dictionary<string, object>());
        }

        public static void BindQueue(
            this IModel channel,
            string queue,
            string exchange)
        {
            channel.QueueBind(
                queue,
                exchange,
                string.Empty,
                new Dictionary<string, object>());
        }

        public static void BindQueue(
            this IModel channel,
            string queue,
            IEnumerable<string> exchanges)
        {
            foreach (var exchange in exchanges)
            {
                BindQueue(channel, queue, exchange);
            }
        }
    }
}