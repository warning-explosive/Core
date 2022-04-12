namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Contract;
    using CrossCuttingConcerns.Settings;

    [ComponentOverride]
    internal class EndpointSettingsScopeProvider : ISettingsScopeProvider,
                                                   IResolvable<ISettingsScopeProvider>
    {
        private readonly EndpointIdentity _endpointIdentity;

        public EndpointSettingsScopeProvider(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public string? Scope => _endpointIdentity.LogicalName;
    }
}