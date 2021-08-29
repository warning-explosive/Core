namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Enumerations;

    internal class ComponentOverridesContainer : Api.Abstractions.IComponentsOverrideContainer,
                                                 Abstractions.IComponentsOverrideContainer
    {
        private readonly List<ComponentOverrideInfo> _overrides;

        public ComponentOverridesContainer()
        {
            _overrides = new List<ComponentOverrideInfo>();
        }

        public IReadOnlyCollection<ComponentOverrideInfo> Overrides => _overrides;

        public Api.Abstractions.IComponentsOverrideContainer Override(
            Type service,
            Type implementation,
            Type replacement,
            EnLifestyle lifestyle)
        {
            _overrides.Add(new ComponentOverrideInfo(service, implementation, replacement, lifestyle));
            return this;
        }
    }
}