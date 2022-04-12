namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using CrossCuttingConcerns.Settings;

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