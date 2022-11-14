namespace SpaceEngineers.Core.Test.WebApplication.StartupActions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics.Attributes;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.RabbitMQ.Extensions;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Settings;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before("SpaceEngineers.Core.GenericEndpoint.Host SpaceEngineers.Core.GenericEndpoint.Host.StartupActions.GenericEndpointHostStartupAction")]
    internal class PurgeRabbitMqQueuesHostStartupAction : IHostStartupAction,
                                                          ICollectionResolvable<IHostStartupAction>,
                                                          IResolvable<PurgeRabbitMqQueuesHostStartupAction>
    {
        private readonly ISettingsProvider<RabbitMqSettings> _rabbitMqSettingsProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;

        public PurgeRabbitMqQueuesHostStartupAction(
            ISettingsProvider<RabbitMqSettings> rabbitMqSettingsProvider,
            IJsonSerializer jsonSerializer,
            ILogger logger)
        {
            _rabbitMqSettingsProvider = rabbitMqSettingsProvider;
            _jsonSerializer = jsonSerializer;
            _logger = logger;
        }

        public async Task Run(CancellationToken token)
        {
            var rabbitMqSettings = await _rabbitMqSettingsProvider
               .Get(token)
               .ConfigureAwait(false);

            await rabbitMqSettings
               .PurgeMessages(_jsonSerializer, _logger, token)
               .ConfigureAwait(false);
        }
    }
}