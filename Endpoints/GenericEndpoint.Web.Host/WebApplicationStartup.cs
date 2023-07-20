namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using Authorization.Web.Handlers;
    using Basics;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Contract;
    using GenericEndpoint.Host.Builder;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.OpenApi.Models;

    /// <summary>
    /// WebApplicationStartup
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification ="web application composition root")]
    public class WebApplicationStartup : BaseStartup
    {
        /// <inheritdoc />
        public WebApplicationStartup(
            IHostBuilder hostBuilder,
            IConfiguration configuration,
            EndpointIdentity endpointIdentity,
            Func<IEndpointBuilder, EndpointOptions> optionsFactory)
            : base(hostBuilder, configuration, endpointIdentity, optionsFactory)
        {
        }

        /// <inheritdoc />
        // TODO: #225 - consider as separate template | project
        protected sealed override void ConfigureAspNetCoreServices(
            IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection.AddMvcCore(options => options.Filters.Add(new AuthorizeFilter()));

            serviceCollection.AddControllers();

            serviceCollection.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(nameof(SwaggerGenOptionsExtensions.SwaggerDoc), new OpenApiInfo
                {
                    Version = EndpointIdentity.Version,
                    Title = EndpointIdentity.LogicalName,
                    Description = "An ASP.NET Core Web API for {0}".Format(EndpointIdentity.LogicalName),
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

                var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                var xmlFileName = $"{assemblyName}.xml";
                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

                options.IncludeXmlComments(xmlFilePath);
            });

            serviceCollection.AddResponseCompression(options => options.Providers.Add<GzipCompressionProvider>());
            serviceCollection.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);

            serviceCollection.AddAuth(configuration);
        }

        /// <inheritdoc />
        protected sealed override void ConfigureAspNetCoreRequestPipeline(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            if (environment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();

                applicationBuilder.UseSwagger();
                applicationBuilder.UseSwaggerUI();
            }
            else
            {
                applicationBuilder.UseExceptionHandler("/Error");
                applicationBuilder.UseHsts();
            }

            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseCookiePolicy();

            applicationBuilder.UseRouting();
            applicationBuilder.UseRequestLocalization();
            applicationBuilder.UseCors(options => options
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();

            applicationBuilder.UseResponseCompression();
            applicationBuilder.UseResponseCaching();

            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}