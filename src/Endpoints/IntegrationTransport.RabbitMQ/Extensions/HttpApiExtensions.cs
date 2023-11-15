namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Logging;
    using HttpApi;
    using Microsoft.Extensions.Logging;
    using RestSharp;
    using RestSharp.Authenticators;
    using Settings;

    /// <summary>
    /// HttpApiExtensions
    /// </summary>
    public static class HttpApiExtensions
    {
        /// <summary>
        /// Creates virtual host if not exists
        /// </summary>
        /// <param name="rabbitMqSettings">RabbitMqSettings</param>
        /// <param name="jsonSerializer">IJsonSerializer</param>
        /// <param name="logger">ILogger</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>Ongoing operation</returns>
        public static async Task CreateVirtualHost(
            this RabbitMqSettings rabbitMqSettings,
            IJsonSerializer jsonSerializer,
            ILogger logger,
            CancellationToken token)
        {
            var options = new RestClientOptions
            {
                Authenticator = new HttpBasicAuthenticator(rabbitMqSettings.User, rabbitMqSettings.Password)
            };

            using (var client = new RestClient(options))
            {
                HashSet<string>? virtualHosts = null;

                foreach (var host in rabbitMqSettings.Hosts)
                {
                    virtualHosts = await ReadVirtualHosts(
                            logger,
                            jsonSerializer,
                            client,
                            host,
                            rabbitMqSettings.HttpApiPort,
                            token)
                       .ConfigureAwait(false);

                    if (virtualHosts != null)
                    {
                        break;
                    }
                }

                if (virtualHosts == null)
                {
                    throw new InvalidOperationException("Unable to read virtual hosts");
                }

                if (virtualHosts.Contains(rabbitMqSettings.VirtualHost))
                {
                    return;
                }

                var created = false;

                foreach (var host in rabbitMqSettings.Hosts)
                {
                    created = await CreateVirtualHostInternal(
                            logger,
                            client,
                            host,
                            rabbitMqSettings.HttpApiPort,
                            rabbitMqSettings.VirtualHost,
                            token)
                       .ConfigureAwait(false);

                    if (created)
                    {
                        break;
                    }
                }

                if (!created)
                {
                    throw new InvalidOperationException($"Unable to create virtual host {rabbitMqSettings.VirtualHost}");
                }

                static async Task<HashSet<string>?> ReadVirtualHosts(
                    ILogger logger,
                    IJsonSerializer jsonSerializer,
                    RestClient client,
                    string host,
                    int httpApiPort,
                    CancellationToken token)
                {
                    var url = $"http://{host}:{httpApiPort}/api/vhosts";

                    var request = new RestRequest(url, Method.Get);

                    request.AddHeader("content-type", MediaTypeNames.Application.Json);

                    var response = await client
                       .ExecuteAsync(request, token)
                       .ConfigureAwait(false);

                    if (response.IsSuccessful)
                    {
                        return jsonSerializer
                           .DeserializeObject<RabbitMqVirtualHost[]>(response.Content!)
                           .Select(host => host.Name)
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    }

                    logger.Warning($"Unable to read virtual hosts: {url} - {response.StatusCode} - {response.ErrorMessage}");
                    return null;
                }

                static async Task<bool> CreateVirtualHostInternal(
                    ILogger logger,
                    RestClient client,
                    string host,
                    int httpApiPort,
                    string virtualHost,
                    CancellationToken token)
                {
                    var url = $"http://{host}:{httpApiPort}/api/vhosts/{virtualHost}";

                    var request = new RestRequest(url, Method.Put);

                    request.AddHeader("content-type", MediaTypeNames.Application.Json);

                    var response = await client
                       .ExecuteAsync(request, token)
                       .ConfigureAwait(false);

                    if (response.IsSuccessful)
                    {
                        return true;
                    }

                    logger.Warning($"Unable to create virtual host: {url} - {response.StatusCode} - {response.ErrorMessage}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Purges queues in virtual host
        /// </summary>
        /// <param name="rabbitMqSettings">RabbitMqSettings</param>
        /// <param name="jsonSerializer">IJsonSerializer</param>
        /// <param name="logger">ILogger</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>Ongoing operation</returns>
        public static async Task PurgeMessages(
            this RabbitMqSettings rabbitMqSettings,
            IJsonSerializer jsonSerializer,
            ILogger logger,
            CancellationToken token)
        {
            var options = new RestClientOptions
            {
                Authenticator = new HttpBasicAuthenticator(rabbitMqSettings.User, rabbitMqSettings.Password)
            };

            using (var client = new RestClient(options))
            {
                HashSet<string>? queues = null;

                foreach (var host in rabbitMqSettings.Hosts)
                {
                    queues = await ReadQueues(
                            logger,
                            jsonSerializer,
                            client,
                            host,
                            rabbitMqSettings.HttpApiPort,
                            rabbitMqSettings.VirtualHost,
                            token)
                       .ConfigureAwait(false);

                    if (queues != null)
                    {
                        break;
                    }
                }

                if (queues == null)
                {
                    throw new InvalidOperationException("Unable to read queues");
                }

                foreach (var queue in queues)
                {
                    var purged = false;

                    foreach (var host in rabbitMqSettings.Hosts)
                    {
                        purged = await PurgeQueue(
                                logger,
                                client,
                                host,
                                rabbitMqSettings.HttpApiPort,
                                rabbitMqSettings.VirtualHost,
                                queue,
                                token)
                           .ConfigureAwait(false);

                        if (purged)
                        {
                            break;
                        }
                    }

                    if (!purged)
                    {
                        throw new InvalidOperationException($"Unable to purge queue {queue}");
                    }
                }
            }

            [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
            static async Task<HashSet<string>?> ReadQueues(
                ILogger logger,
                IJsonSerializer jsonSerializer,
                RestClient client,
                string host,
                int httpApiPort,
                string virtualHost,
                CancellationToken token)
            {
                var url = $"http://{host}:{httpApiPort}/api/queues/{virtualHost}";

                var request = new RestRequest(url, Method.Get);

                request.AddHeader("content-type", MediaTypeNames.Application.Json);

                var response = await client
                   .ExecuteAsync(request, token)
                   .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    return jsonSerializer
                       .DeserializeObject<RabbitMqQueue[]>(response.Content!)
                       .Select(queue => queue.Name)
                       .ToHashSet(StringComparer.OrdinalIgnoreCase);
                }

                if (!response.IsSuccessful && response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                logger.Warning($"Unable to read queues: {url} - {response.StatusCode} - {response.ErrorMessage}");
                return null;
            }

            static async Task<bool> PurgeQueue(
                ILogger logger,
                RestClient client,
                string host,
                int httpApiPort,
                string virtualHost,
                string queue,
                CancellationToken token)
            {
                var url = $"http://{host}:{httpApiPort}/api/queues/{virtualHost}/{queue}/contents";

                var request = new RestRequest(url, Method.Delete);

                request.AddHeader("Accept", "*/*");
                request.AddHeader("Accept-Encoding", "gzip, deflate, br");
                request.AddBody($@"{{""vhost"":""{virtualHost}"",""name"":""{queue}"",""mode"":""purge""}}", MediaTypeNames.Application.Json);

                var response = await client
                   .ExecuteAsync(request, token)
                   .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    return true;
                }

                logger.Warning($"Unable to purge queues: {url} - {response.StatusCode} - {response.ErrorMessage}");
                return false;
            }
        }
    }
}