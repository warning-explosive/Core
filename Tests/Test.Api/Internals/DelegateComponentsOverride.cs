namespace SpaceEngineers.Core.Test.Api.Internals
{
    using System;
    using CompositionRoot.Api.Abstractions;

    internal class DelegateComponentsOverride : IComponentsOverride
    {
        private readonly Action<IComponentsOverrideContainer> _overrideAction;

        public DelegateComponentsOverride(Action<IComponentsOverrideContainer> overrideAction)
        {
            _overrideAction = overrideAction;
        }

        public void RegisterOverrides(IComponentsOverrideContainer container)
        {
            _overrideAction(container);
        }

        public override int GetHashCode()
        {
            return _overrideAction.GetHashCode();
        }
    }
}