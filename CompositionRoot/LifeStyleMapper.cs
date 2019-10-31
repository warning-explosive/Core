namespace SpaceEngineers.Core.CompositionRoot
{
    using System.Collections.Generic;
    using System.Linq;
    using Enumerations;
    using SimpleInjector;

    internal static class LifeStyleMapper
    {
        private static readonly IDictionary<EnLifestyle, Lifestyle> _mapping
            = new Dictionary<EnLifestyle, Lifestyle>
              {
                  [EnLifestyle.Transient] = Lifestyle.Transient,
                  [EnLifestyle.Singleton] = Lifestyle.Singleton,
                  [EnLifestyle.Scoped] = Lifestyle.Scoped,
              };

        internal static Lifestyle MapLifestyle(EnLifestyle enLifestyle)
        {
            return _mapping[enLifestyle];
        }
        
        internal static EnLifestyle MapLifestyle(Lifestyle enLifestyle)
        {
            return _mapping.Single(pair => pair.Value == enLifestyle).Key;
        }
    }
}