namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class TestSettingsScopeProvider : ISettingsScopeProvider,
                                               IResolvable<ISettingsScopeProvider>
    {
        public TestSettingsScopeProvider(string scope)
        {
            Scope = scope;
        }

        public string? Scope { get; }
    }
}