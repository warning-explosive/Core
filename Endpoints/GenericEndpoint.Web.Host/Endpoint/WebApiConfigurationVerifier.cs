namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Endpoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Verifiers;
    using Contract;
    using Contract.Attributes;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;

    [Component(EnLifestyle.Singleton)]
    internal class WebApiConfigurationVerifier : IConfigurationVerifier,
                                                 ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly EndpointIdentity _endpointIdentity;

        public WebApiConfigurationVerifier(
            ITypeProvider typeProvider,
            EndpointIdentity endpointIdentity)
        {
            _typeProvider = typeProvider;
            _endpointIdentity = endpointIdentity;
        }

        public void Verify()
        {
            var exceptions = new List<Exception>();

            var controllers = _typeProvider
                .OurTypes
                .Where(type => type.IsController());

            VerifyControllers(_endpointIdentity, controllers, exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void VerifyControllers(
            EndpointIdentity endpointIdentity,
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

                if (!uniqueControllers.Add(controller.GetControllerName()))
                {
                    exceptions.Add(new InvalidOperationException($"Web-api controller {controller.FullName} should have unique name"));
                }

                var groupNameAttribute = controller.GetAttribute<EndpointGroupNameAttribute>();

                if (groupNameAttribute == null)
                {
                    exceptions.Add(new InvalidOperationException($"Web-api controller {controller.FullName} should be marked by {nameof(EndpointGroupNameAttribute)}"));
                }

                if (!string.Equals(groupNameAttribute.EndpointGroupName, endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase))
                {
                    exceptions.Add(new InvalidOperationException($"Web-api controller {controller.FullName} should have {nameof(EndpointGroupNameAttribute)}.{nameof(EndpointGroupNameAttribute.EndpointGroupName)} equal to endpoint's logical name"));
                }

                var actions = controller
                    .GetControllerActions()
                    .Select(pair => pair.Action);

                VerifyActions(controller, actions, exceptions);
            }
        }

        private static void VerifyActions(
            Type controller,
            IEnumerable<MethodInfo> actions,
            ICollection<Exception> exceptions)
        {
            foreach (var action in actions)
            {
                if (!action.IsAnonymousAction(controller)
                    && !action.GetActionFeatures(controller).Any())
                {
                    exceptions.Add(new InvalidOperationException($"Web-api method {action.Name} or containing controller {controller.FullName} should be marked by {nameof(FeatureAttribute)}"));
                }

                if (!action.HasAttribute<HttpMethodAttribute>())
                {
                    exceptions.Add(new InvalidOperationException($"Web-api method {action.Name} or containing controller {controller.FullName} should be marked by one of {nameof(HttpMethodAttribute)}'s ancestors"));
                }

                if (!controller.GetAttribute<RouteAttribute>().Template.StartsWith("api")
                    && !action.GetAttribute<RouteAttribute>().Template.StartsWith("api"))
                {
                    exceptions.Add(new InvalidOperationException($"Web-api method {action.Name} or containing controller {controller.FullName} should start route pattern from 'api' prefix"));
                }
            }
        }
    }
}