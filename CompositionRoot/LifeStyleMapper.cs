namespace SpaceEngineers.Core.CompositionRoot
{
    using System.Collections.Generic;
    using System.Linq;
    using Enumerations;
    using SimpleInjector;

    internal static class LifeStyleMapper
    {
        private static readonly IDictionary<EnLifestyle, Lifestyle> Mapping
            = new Dictionary<EnLifestyle, Lifestyle>
            {
                [EnLifestyle.Transient] = Lifestyle.Transient,
                [EnLifestyle.Singleton] = Lifestyle.Singleton,
                [EnLifestyle.Scoped] = Lifestyle.Scoped,
            };

        internal static Lifestyle MapLifestyle(EnLifestyle lifestyle)
        {
            return Mapping[lifestyle];
        }

        internal static EnLifestyle MapLifestyle(Lifestyle lifestyle)
        {
            return Mapping.Single(pair => pair.Value == lifestyle).Key;
        }
    }
}