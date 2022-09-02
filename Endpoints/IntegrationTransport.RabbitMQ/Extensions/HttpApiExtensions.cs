namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Extensions
{
    using System;
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossCuttingConcerns.Extensions;
    using CrossCuttingConcerns.Json;
    using HttpApi;
    using Microsoft.Extensions.Logging;
    using RestSharp;
    using RestSharp.Authenticators;
    using Settings;

    internal static class HttpApiExtensions
    {
        public static async Task<VirtualHost[]> ReadVirtualHosts(
            this RabbitMqSettings rabbitMqSettings,
            IJsonSerializer jsonSerializer,
            ILogger logger,
            CancellationToken token)
        {
            using (var client = new RestClient())
            {
                client.Authenticator = new HttpBasicAuthenticator(rabbitMqSettings.User, rabbitMqSettings.Password);

                RestResponse? response = default;

                foreach (var host in rabbitMqSettings.Hosts)
                {
                    var url = $"http://{host}:{rabbitMqSettings.HttpApiPort}/api/vhosts";

                    var request = new RestRequest(url, Method.Get);

                    response = await client
                       .ExecuteAsync(request, token)
                       .ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return jsonSerializer.DeserializeObject<VirtualHost[]>(response.Content!);
                    }

                    logger.Warning($"RabbitMQ host unavailable: {url}");
                }

                throw new InvalidOperationException(response?.ToString() ?? "Unable to read virtual hosts");
            }
        }

        public static async Task CreateVirtualHost(
            this RabbitMqSettings rabbitMqSettings,
            ILogger logger,
            CancellationToken token)
        {
            using (var client = new RestClient())
            {
                client.Authenticator = new HttpBasicAuthenticator(rabbitMqSettings.User, rabbitMqSettings.Password);

                RestResponse? response = default;

                foreach (var host in rabbitMqSettings.Hosts)
                {
                    var url = $"http://{host}:{rabbitMqSettings.HttpApiPort}/api/vhosts/{rabbitMqSettings.VirtualHost}";

                    var request = new RestRequest(url, Method.Put);

                    request.AddHeader("content-type", MediaTypeNames.Application.Json);

                    response = await client
                       .ExecuteAsync(request, token)
                       .ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        return;
                    }

                    logger.Warning($"RabbitMQ host unavailable: {url}");
                }

                throw new InvalidOperationException(response?.ToString() ?? $"Unable to create virtual host {rabbitMqSettings.VirtualHost}");
            }
        }
    }
}