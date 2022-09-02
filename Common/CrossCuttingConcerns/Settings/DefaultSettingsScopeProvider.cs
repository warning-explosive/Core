namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class DefaultSettingsScopeProvider : ISettingsScopeProvider,
                                                  IResolvable<ISettingsScopeProvider>
    {
        public string? Scope => default;
    }
}