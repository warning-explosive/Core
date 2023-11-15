namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class WebApiFeaturesProvider : IWebApiFeaturesProvider
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>> _features;

        public WebApiFeaturesProvider(IReadOnlyCollection<Type> controllers)
        {
            _features = Initialize(controllers);
        }

        public IReadOnlyCollection<string> GetFeatures(string controller, string action, string verb)
        {
            return _features.TryGetValue(controller, out var actions)
                && actions.TryGetValue(action, out var verbs)
                && verbs.TryGetValue(verb, out var features)
                    ? features
                    : Array.Empty<string>();
        }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>> Initialize(
            IReadOnlyCollection<Type> controllers)
        {
            return controllers
                .ToDictionary(
                    controller => controller.GetControllerName(),
                    controller => controller
                        .GetControllerActions()
                        .GroupBy(pair => pair.Action.Name)
                        .ToDictionary(
                            grp => grp.Key,
                            grp => grp
                                .ToDictionary(
                                    pair => pair.Verb,
                                    pair => pair.Action.GetActionFeatures(controller),
                                    StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IReadOnlyCollection<string>>,
                            StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}