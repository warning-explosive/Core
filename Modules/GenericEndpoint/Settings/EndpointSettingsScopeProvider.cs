namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using AutoRegistration.Api.Attributes;
    using Contract;
    using CrossCuttingConcerns.Api.Abstractions;

    [ComponentOverride]
    internal class EndpointSettingsScopeProvider : ISettingsScopeProvider
    {
        private readonly EndpointIdentity _endpointIdentity;

        public EndpointSettingsScopeProvider(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public string? Scope => _endpointIdentity.LogicalName;
    }
}