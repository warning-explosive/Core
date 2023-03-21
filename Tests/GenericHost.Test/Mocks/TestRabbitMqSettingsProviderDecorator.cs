namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System.Reflection;
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

        public RabbitMqSettings Get()
        {
            var rabbitMqSettings = Decoratee.Get();

            typeof(RabbitMqSettings)
                .GetProperty(nameof(RabbitMqSettings.VirtualHost), BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                .SetValue(rabbitMqSettings, _virtualHostProvider.VirtualHost);

            return rabbitMqSettings;
        }

        [ManuallyRegisteredComponent(nameof(TranslationTest))]
        internal class VirtualHostProvider : IResolvable<VirtualHostProvider>
        {
            public VirtualHostProvider(string virtualHost)
            {
                VirtualHost = virtualHost;
            }

            public string VirtualHost { get; }
        }
    }
}