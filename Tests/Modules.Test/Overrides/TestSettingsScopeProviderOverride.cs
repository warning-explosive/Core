namespace SpaceEngineers.Core.Modules.Test.Overrides
{
    using CompositionRoot.Registration;
    using CrossCuttingConcerns.Settings;
    using Mocks;

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