namespace SpaceEngineers.Core.GenericHost.Test.StartupActions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics.Attributes;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.GenericEndpoint.Host.StartupActions;
    using SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Extensions;
    using SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Settings;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostStartupAction))]
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