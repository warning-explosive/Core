namespace SpaceEngineers.Core.CompositionRoot.Registration
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
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container);
    }
}