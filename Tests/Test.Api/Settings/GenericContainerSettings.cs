namespace SpaceEngineers.Core.Test.Api.Settings
{
    /// <summary>
    /// GenericContainerSettings
    /// </summary>
    public class GenericContainerSettings
    {
        /// <summary> .cctor </summary>
        public GenericContainerSettings()
        {
            DependencyContainerImplementationFullName = "SpaceEngineers.Core.CompositionRoot.SimpleInjector.Internals.SimpleInjectorDependencyContainerImplementation";
        }

        /// <summary>
        /// DependencyContainer implementation FullName
        /// </summary>
        public string DependencyContainerImplementationFullName { get; set; }
    }
}