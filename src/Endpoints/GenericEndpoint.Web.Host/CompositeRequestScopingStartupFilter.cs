namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Basics;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using SimpleInjector;
    using SimpleInjector.Integration.AspNetCore;
    using SimpleInjector.Lifestyles;
    using Swagger;

    /// <summary>
    /// SimpleInjector.Integration.AspNetCore.RequestScopingStartupFilter
    /// </summary>
    internal class CompositeRequestScopingStartupFilter : IStartupFilter
    {
        private static readonly MethodInfo ConnectToScopeMethod = new MethodFinder(
            typeof(SimpleInjectorHttpContextExtensions),
            "ConnectToScope",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod)
        {
            ArgumentTypes = new[] { typeof(HttpContext), typeof(Scope) }
        }.FindMethod() ?? throw new InvalidOperationException($"Could not find {nameof(SimpleInjectorHttpContextExtensions)}.ConnectToScope() method");

        private readonly IReadOnlyDictionary<Type, Container> _containers;
        private readonly ILogger<CompositeRequestScopingStartupFilter> _logger;

        public CompositeRequestScopingStartupFilter(
            IReadOnlyDictionary<Type, Container> containers,
            ILogger<CompositeRequestScopingStartupFilter> logger)
        {
            _containers = containers;
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                ConfigureRequestScoping(builder);
                next(builder);
            };
        }

#pragma warning disable CA2007
        private void ConfigureRequestScoping(IApplicationBuilder builder)
        {
            builder.Use(async (httpContext, next) =>
            {
                if (httpContext.Request.IsSwaggerRequest())
                {
                    await next();
                    return;
                }

                var container = SelectContainer(httpContext);

                var flowing = container.Options.DefaultScopedLifestyle == ScopedLifestyle.Flowing;

                var scope = flowing
                    ? new Scope(container)
                    : AsyncScopedLifestyle.BeginScope(container);

                try
                {
                    ConnectToScopeMethod.Invoke(null, new object?[] { httpContext, scope });
                    await next();
                }
                finally
                {
                    await scope.DisposeScopeAsync();
                }

                scope = null;
            });
        }
#pragma warning restore CA2007

        private Container SelectContainer(HttpContext httpContext)
        {
            if (!httpContext.TryGetEndpoint(out var endpoint))
            {
                throw new InvalidOperationException("Unable to find route endpoint in order to select service scope for incoming request");
            }

            var controller = endpoint.GetControllerType();

            if (!_containers.TryGetValue(controller, out var container))
            {
                throw new InvalidOperationException("service scope for incoming request wasn't resolved");
            }

            return container;
        }
    }
}