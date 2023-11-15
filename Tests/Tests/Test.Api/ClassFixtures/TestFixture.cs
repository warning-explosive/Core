namespace SpaceEngineers.Core.Test.Api.ClassFixtures
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Registrations;

    /// <summary>
    /// TestFixture
    /// </summary>
    public sealed class TestFixture : IModulesTestFixture
    {
        private static readonly ConcurrentDictionary<int, IDependencyContainer> Cache = new ConcurrentDictionary<int, IDependencyContainer>();

        /// <inheritdoc />
        public IHostBuilder CreateHostBuilder()
        {
            return Host
                .CreateDefaultBuilder()
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    builder
                        .AddSimpleConsole(options =>
                        {
                            options.ColorBehavior = LoggerColorBehavior.Disabled;
                            options.SingleLine = false;
                            options.IncludeScopes = false;
                            options.TimestampFormat = null;
                        })
                        .SetMinimumLevel(LogLevel.Trace);
                });
        }

        /// <inheritdoc />
        public IManualRegistration DelegateRegistration(Action<IManualRegistrationsContainer> registrationAction)
        {
            return new ManualDelegateRegistration(registrationAction);
        }

        /// <inheritdoc />
        public IComponentsOverride DelegateOverride(Action<IRegisterComponentsOverrideContainer> overrideAction)
        {
            return new DelegateComponentsOverride(overrideAction);
        }

        /// <inheritdoc />
        public IDependencyContainer DependencyContainer(DependencyContainerOptions options)
        {
            var hash = DependencyContainerHash(options);

            if (Cache.TryGetValue(hash, out var container))
            {
                return container;
            }

            container = CompositionRoot.DependencyContainer.Create(options);

            return Cache.AddOrUpdate(hash, _ => container, (_, _) => container);
        }

        private static int DependencyContainerHash(
            DependencyContainerOptions options,
            params Assembly[] aboveAssembly)
        {
            return HashCode.Combine(aboveAssembly.Aggregate(int.MaxValue, HashCode.Combine), options);
        }
    }
}