namespace SpaceEngineers.Core.Utilities.SettingsManager
{
    using System;
    using System.Linq;
    using CompositionRoot;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using Extensions;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class SettingsMangerImpl : ISettingsManger
    {
        public TSettings Get<TSettings>()
            where TSettings : ISettings, new()
        {
            if (typeof(TSettings).IsImplementationOfOpenGenericInterface(typeof(IFileSystemSettings<>)))
            {
                return GetFormatterInstance<TSettings>().Deserialize<TSettings>().Result;
            }
            
            throw new NotSupportedException(typeof(TSettings).FullName);
        }

        public void Set<TSettings>(TSettings value)
            where TSettings : ISettings, new()
        {
            if (typeof(TSettings).IsImplementationOfOpenGenericInterface(typeof(IFileSystemSettings<>)))
            {
                GetFormatterInstance<TSettings>().Serialize(value).Wait();
                
                return;
            }
            
            throw new NotSupportedException(typeof(TSettings).FullName);
        }

        private static IAsyncFormatter GetFormatterInstance<TSettings>()
            where TSettings : ISettings, new()
        {
            var formatterType = typeof(TSettings).GetInterfaces()
                                                 .Where(i => i.IsGenericType)
                                                 .Single(i => i.GetGenericTypeDefinition() == typeof(IFileSystemSettings<>))
                                                 .GetGenericArguments()[0];

            return (IAsyncFormatter)DependencyContainer.Resolve(formatterType);
        }
    }
}