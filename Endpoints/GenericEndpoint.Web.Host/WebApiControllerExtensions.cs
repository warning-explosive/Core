namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using Contract.Attributes;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;

    internal static class WebApiControllerExtensions
    {
        public static bool IsController(this Type type)
        {
            return typeof(ControllerBase).IsAssignableFrom(type) && type.IsConcreteType();
        }

        public static string GetControllerName(this Type controller)
        {
            return controller.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                ? controller.Name[..^10]
                : controller.Name;
        }

        public static IEnumerable<(string Verb, MethodInfo Action)> GetControllerActions(this Type controller)
        {
            return controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.HasAttribute<HttpMethodAttribute>())
                .SelectMany(action => action
                    .GetCustomAttributes<HttpMethodAttribute>()
                    .SelectMany(attribute => attribute
                        .HttpMethods
                        .Select(verb => (verb, action))));
        }

        public static bool IsAnonymousAction(this MethodInfo action, Type controller)
        {
            return controller.HasAttribute<AllowAnonymousAttribute>()
                   || action.HasAttribute<AllowAnonymousAttribute>();
        }

        public static IReadOnlyCollection<string> GetActionFeatures(this MethodInfo action, Type controller)
        {
            if (action.IsAnonymousAction(controller))
            {
                return Array.Empty<string>();
            }

            var controllerFeatures = controller.GetAttribute<FeatureAttribute>()?.Features;
            var actionFeatures = action.GetAttribute<FeatureAttribute>()?.Features;

            return (actionFeatures ?? Enumerable.Empty<string>())
                .Concat(controllerFeatures ?? Enumerable.Empty<string>())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}