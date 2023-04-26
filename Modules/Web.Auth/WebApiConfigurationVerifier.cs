namespace SpaceEngineers.Core.Web.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Verifiers;
    using GenericEndpoint.Contract.Attributes;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using AllowAnonymousAttribute = Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute;
    using TypeExtensions = Basics.TypeExtensions;

    [Component(EnLifestyle.Singleton)]
    internal class WebApiConfigurationVerifier : IConfigurationVerifier,
                                                 ICollectionResolvable<IConfigurationVerifier>,
                                                 IWebApiFeaturesProvider,
                                                 IResolvable<IWebApiFeaturesProvider>
    {
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>> _features
            = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<string> GetFeatures(string controller, string action, string verb)
        {
            return _features.TryGetValue(controller, out var actions)
                && actions.TryGetValue(action, out var verbs)
                && verbs.TryGetValue(verb, out var features)
                    ? features
                    : Array.Empty<string>();
        }

        public void Verify()
        {
            var exceptions = new List<Exception>();

            var controllers = TypeExtensions
                .AllTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && type.IsConcreteType())
                .ToList();

            VerifyControllers(controllers, exceptions);

            _features = controllers
                .ToDictionary(
                    GetControllerName,
                    controller => controller
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(method => method.HasAttribute<HttpMethodAttribute>())
                        .SelectMany(action => action
                            .GetCustomAttributes<HttpMethodAttribute>()
                            .SelectMany(attribute => attribute
                                .HttpMethods
                                .Select(verb => (verb, action))))
                        .GroupBy(pair => pair.action.Name)
                        .ToDictionary(
                            grp => grp.Key,
                            grp => grp
                                .ToDictionary(
                                    pair => pair.verb,
                                    pair => GetFeatures(controller, pair.action, exceptions),
                                    StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IReadOnlyCollection<string>>,
                            StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>,
                    StringComparer.OrdinalIgnoreCase);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void VerifyControllers(
            IEnumerable<Type> controllers,
            ICollection<Exception> exceptions)
        {
            var uniqueControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var controller in controllers)
            {
                if (!controller.Name.EndsWith("Controller", StringComparison.Ordinal))
                {
                    exceptions.Add(new InvalidOperationException($"Web-api controller {controller.FullName} should have type name with suffix 'Controller'"));
                }

                if (!uniqueControllers.Add(GetControllerName(controller)))
                {
                    exceptions.Add(new InvalidOperationException($"Web-api controller {controller.FullName} should have unique name"));
                }
            }
        }

        private static IReadOnlyCollection<string> GetFeatures(
            Type controller,
            MethodInfo action,
            ICollection<Exception> exceptions)
        {
            if (controller.HasAttribute<AllowAnonymousAttribute>()
                || action.HasAttribute<AllowAnonymousAttribute>())
            {
                return Array.Empty<string>();
            }

            var controllerFeatures = controller.GetAttribute<FeatureAttribute>()?.Features;
            var actionFeatures = action.GetAttribute<FeatureAttribute>()?.Features;

            if ((actionFeatures == null || !actionFeatures.Any())
                && (controllerFeatures == null || !controllerFeatures.Any()))
            {
                exceptions.Add(new InvalidOperationException($"Web-api method {action.Name} or containing controller {controller.FullName} should be marked by {nameof(FeatureAttribute)}"));
            }

            return (actionFeatures ?? Enumerable.Empty<string>())
                .Concat(controllerFeatures ?? Enumerable.Empty<string>())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static string GetControllerName(Type controller)
        {
            return controller.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                ? controller.Name[..^10]
                : controller.Name;
        }
    }
}