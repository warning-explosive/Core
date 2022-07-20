namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using CrossCuttingConcerns.Settings;
    using IntegrationTransport.RabbitMQ.Settings;

    [ManuallyRegisteredComponent(nameof(TranslationTest))]
    internal class TestRabbitMqSettingsProviderDecorator : ISettingsProvider<RabbitMqSettings>,
                                                           IDecorator<ISettingsProvider<RabbitMqSettings>>
    {
        private readonly VirtualHostProvider _virtualHostProvider;

        public TestRabbitMqSettingsProviderDecorator(
            ISettingsProvider<RabbitMqSettings> decoratee,
            VirtualHostProvider virtualHostProvider)
        {
            Decoratee = decoratee;
            _virtualHostProvider = virtualHostProvider;
        }

        public ISettingsProvider<RabbitMqSettings> Decoratee { get; }

        public async Task<RabbitMqSettings> Get(CancellationToken token)
        {
            var rabbitMqSettings = await Decoratee
               .Get(token)
               .ConfigureAwait(false);

            rabbitMqSettings.VirtualHost = _virtualHostProvider.VirtualHost;

            return rabbitMqSettings;
        }

        [ManuallyRegisteredComponent(nameof(TranslationTest))]
        internal class VirtualHostProvider : IResolvable<VirtualHostProvider>
        {
            private readonly string _virtualHost;

            public VirtualHostProvider(string virtualHost)
            {
                _virtualHost = virtualHost;
            }

            public string VirtualHost => _virtualHost;
        }
    }
}