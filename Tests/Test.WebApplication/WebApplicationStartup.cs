namespace SpaceEngineers.Core.WebApplication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using IntegrationTransport.Host.Builder;
    using IntegrationTransport.WebHost.SimpleInjector;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Web.Auth;
    using Web.Auth.Authentication;

    internal class WebApplicationStartup : SimpleInjectorBaseStartup
    {
        private readonly IConfiguration _configuration;

        public WebApplicationStartup(
            IConfiguration configuration,
            Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory)
            : base(optionsFactory)
        {
            _configuration = configuration;
        }

        protected sealed override void ConfigureCoreServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvc(options => options.Filters.Add(new AuthorizeFilter()));

            serviceCollection.AddControllers().AddNewtonsoftJson();

            serviceCollection.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Investment API",
                    Description = "An ASP.NET Core Web API for Investment Web application",
                    TermsOfService = new Uri("https://github.com/warning-explosive/Investment"),
                    Contact = new OpenApiContact
                    {
                        Name = "Contacts",
                        Url = new Uri("https://github.com/warning-explosive/Investment")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License",
                        Url = new Uri("https://github.com/warning-explosive/Investment")
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

                var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
                options.IncludeXmlComments(xmlFilePath);
            });

            serviceCollection.AddSwaggerGenNewtonsoftSupport();

            serviceCollection.AddResponseCompression(options => options.Providers.Add<GzipCompressionProvider>());
            serviceCollection.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);

            serviceCollection.AddAuth(_configuration);
        }

        protected sealed override void ConfigureRequestPipeline(
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