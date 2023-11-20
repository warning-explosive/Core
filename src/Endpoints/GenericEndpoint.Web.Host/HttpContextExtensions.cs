namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Template;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// HttpContext extensions
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets controller type from HttpContext Endpoint
        /// </summary>
        /// <param name="endpoint">Microsoft.AspNetCore.Http.Endpoint</param>
        /// <returns>Controller type</returns>
        public static Type GetControllerType(this Microsoft.AspNetCore.Http.Endpoint endpoint)
        {
            return endpoint
                .Metadata
                .GetMetadata<ControllerActionDescriptor>()
                .ControllerTypeInfo
                .AsType();
        }

        /// <summary>
        /// Gets Endpoint from HttpContext
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="endpoint">Microsoft.AspNetCore.Http.Endpoint</param>
        /// <returns>Possibly found Microsoft.AspNetCore.Http.Endpoint</returns>
        public static bool TryGetEndpoint(
            this HttpContext httpContext,
            [NotNullWhen(true)] out Microsoft.AspNetCore.Http.Endpoint? endpoint)
        {
            endpoint = httpContext.GetEndpoint() ?? MatchEndpoint(httpContext);

            return endpoint != null;

            static Microsoft.AspNetCore.Http.Endpoint? MatchEndpoint(HttpContext httpContext)
            {
                var endpoint = httpContext
                    .RequestServices
                    .GetRequiredService<EndpointDataSource>()
                    .Endpoints
                    .OfType<RouteEndpoint>()
                    .Where(Predicate(httpContext))
                    .InformativeSingleOrDefault(Amb);

                return endpoint;

                static Func<RouteEndpoint, bool> Predicate(HttpContext httpContext)
                {
                    return endpoint =>
                    {
                        var templateMatcher = new TemplateMatcher(
                            TemplateParser.Parse(endpoint.RoutePattern.RawText!),
                            new RouteValueDictionary());

                        if (!templateMatcher.TryMatch(httpContext.Request.GetEncodedPathAndQuery(), httpContext.Request.RouteValues))
                        {
                            return false;
                        }

                        var httpMethodAttribute = endpoint.Metadata.GetMetadata<HttpMethodAttribute>();

                        return httpMethodAttribute is not null
                               && httpMethodAttribute.HttpMethods.Any(requestMethod => requestMethod
                                   .Equals(httpContext.Request.Method, StringComparison.OrdinalIgnoreCase));
                    };
                }

                static string Amb(IEnumerable<RouteEndpoint> endpoints)
                {
                    return $"More than one endpoint match to request: {endpoints.ToString(", ")}";
                }
            }
        }
    }
}