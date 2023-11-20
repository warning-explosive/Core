namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Auth;
    using Basics;
    using Contract;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Template;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;

    internal static class ServiceCollectionExtensions
    {
        private static readonly IEnumerable<TemplateMatcher> SwaggerMatchers = new[]
        {
            new TemplateMatcher(
                TemplateParser.Parse("/api/index.html"),
                new RouteValueDictionary()),
            new TemplateMatcher(
                TemplateParser.Parse("/api/swagger-ui-standalone-preset.js"),
                new RouteValueDictionary()),
            new TemplateMatcher(
                TemplateParser.Parse("api/{documentname}/swagger.json"),
                new RouteValueDictionary())
        };

        public static bool IsSwaggerRequest(this HttpRequest request)
        {
            return SwaggerMatchers.Any(templateMatcher => templateMatcher.TryMatch(request.GetEncodedPathAndQuery(), request.RouteValues));
        }

        public static IServiceCollection AddSwagger(
            this IServiceCollection serviceCollection,
            IEnumerable<EndpointIdentity> endpointIdentities)
        {
            return serviceCollection.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = @"JWT authentication header using the bearer scheme. Example: 'bearer your_token'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

                options.AddSecurityDefinition(BasicDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = @"Basic authentication header using the basic scheme. Example: 'basic your_username_password_as_base_64_byte_array'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = BasicDefaults.AuthenticationScheme
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            },
                            Scheme = JwtBearerDefaults.AuthenticationScheme,
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = BasicDefaults.AuthenticationScheme
                            },
                            Scheme = BasicDefaults.AuthenticationScheme,
                            Name = BasicDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                foreach (var endpointIdentity in endpointIdentities)
                {
                    options.SwaggerDoc(endpointIdentity.LogicalName, new OpenApiInfo
                    {
                        Version = endpointIdentity.Version,
                        Title = endpointIdentity.LogicalName,
                        Description = "An ASP.NET Core Web API for {0}".Format(endpointIdentity.LogicalName),
                        TermsOfService = new Uri("https://github.com/warning-explosive/Core"),
                        Contact = new OpenApiContact
                        {
                            Name = "Contacts",
                            Url = new Uri("https://github.com/warning-explosive/Core")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "License",
                            Url = new Uri("https://github.com/warning-explosive/Core")
                        }
                    });

                    var assemblyName = endpointIdentity.Assembly?.GetName().Name;

                    if (assemblyName.IsNullOrEmpty())
                    {
                        throw new InvalidOperationException($"{endpointIdentity} endpoint should have specified assembly");
                    }

                    var xmlFileName = $"{assemblyName}.xml";
                    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

                    if (!File.Exists(xmlFilePath))
                    {
                        throw new InvalidOperationException($"Unable to find documentation file {xmlFilePath}");
                    }

                    options.IncludeXmlComments(xmlFilePath);
                }
            });
        }
    }
}