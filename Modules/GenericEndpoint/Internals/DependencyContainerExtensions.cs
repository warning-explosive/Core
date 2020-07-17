namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using SettingsManager.Abstractions;

    internal static class DependencyContainerExtensions
    {
        internal static Task<TSetting> GetSetting<TSetting>(this IDependencyContainer dependencyContainer)
            where TSetting : class, ISettings
        {
            return dependencyContainer.Resolve<ISettingsManager<TSetting>>().Get();
        }
    }
}