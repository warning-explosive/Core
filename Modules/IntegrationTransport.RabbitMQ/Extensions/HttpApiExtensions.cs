namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Extensions
{
    using System;
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossCuttingConcerns.Json;
    using HttpApi;
    using RestSharp;
    using RestSharp.Authenticators;
    using Settings;

    internal static class HttpApiExtensions
    {
        public static async Task<VirtualHost[]> ReadVirtualHosts(
            this RabbitMqSettings rabbitMqSettings,
            IJsonSerializer jsonSerializer,
            CancellationToken token)
        {
            using (var client = new RestClient())
            {
                client.Authenticator = new HttpBasicAuthenticator(rabbitMqSettings.User, rabbitMqSettings.Password);

                var request = new RestRequest(
                    $"http://{rabbitMqSettings.Host}:{rabbitMqSettings.HttpApiPort}/api/vhosts",
                    Method.Get);

                var response = await client
                   .ExecuteAsync(request, token)
                   .ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new InvalidOperationException(response.ToString());
                }

                return jsonSerializer.DeserializeObject<VirtualHost[]>(response.Content!);
            }
        }

        public static async Task CreateVirtualHost(
            this RabbitMqSettings rabbitMqSettings,
            CancellationToken token)
        {
            using (var client = new RestClient())
            {
                client.Authenticator = new HttpBasicAuthenticator(rabbitMqSettings.User, rabbitMqSettings.Password);

                var request = new RestRequest(
                    $"http://{rabbitMqSettings.Host}:{rabbitMqSettings.HttpApiPort}/api/vhosts/{rabbitMqSettings.VirtualHost}",
                    Method.Put);

                request.AddHeader("content-type", MediaTypeNames.Application.Json);

                var response = await client
                   .ExecuteAsync(request, token)
                   .ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new InvalidOperationException(response.ToString());
                }
            }
        }
    }
}