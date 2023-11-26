namespace SpaceEngineers.Core.Test.Api.Registrations
{
    using System;
    using CompositionRoot.Registration;

    internal class DelegateComponentsOverride : IComponentsOverride
    {
        private readonly Action<IRegisterComponentsOverrideContainer> _overrideAction;

        public DelegateComponentsOverride(Action<IRegisterComponentsOverrideContainer> overrideAction)
        {
            _overrideAction = overrideAction;
        }

        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            _overrideAction(container);
        }

        public override int GetHashCode()
        {
            return _overrideAction.GetHashCode();
        }
    }
}