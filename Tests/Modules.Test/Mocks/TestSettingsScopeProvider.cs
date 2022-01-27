namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using AutoRegistration.Api.Attributes;
    using CrossCuttingConcerns.Api.Abstractions;

    [ComponentOverride]
    internal class TestSettingsScopeProvider : ISettingsScopeProvider
    {
        public TestSettingsScopeProvider(string scope)
        {
            Scope = scope;
        }

        public string? Scope { get; }
    }
}