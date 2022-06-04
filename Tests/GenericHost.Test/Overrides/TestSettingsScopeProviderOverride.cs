namespace SpaceEngineers.Core.GenericHost.Test.Overrides
{
    using CrossCuttingConcerns.Settings;
    using Mocks;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

    internal class TestSettingsScopeProviderOverride : IComponentsOverride
    {
        private readonly string _scope;

        public TestSettingsScopeProviderOverride(string scope)
        {
            _scope = scope;
        }

        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.OverrideInstance<ISettingsScopeProvider>(new TestSettingsScopeProvider(_scope));
        }
    }
}