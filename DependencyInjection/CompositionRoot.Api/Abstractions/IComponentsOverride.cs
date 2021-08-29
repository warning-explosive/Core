namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions
{
    /// <summary>
    /// IComponentsOverride
    /// </summary>
    public interface IComponentsOverride
    {
        /// <summary>
        /// Registers overrides manually
        /// </summary>
        /// <param name="container">IComponentsOverrideContainer</param>
        public void RegisterOverrides(IComponentsOverrideContainer container);
    }
}