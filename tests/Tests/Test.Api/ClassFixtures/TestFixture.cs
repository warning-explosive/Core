namespace SpaceEngineers.Core.Test.Api.ClassFixtures;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using DependencyInjection;
using DependencyInjection.Registration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Registrations;

public sealed class TestFixture
{
    private static readonly ConcurrentDictionary<int, IDependencyContainer> Cache = new ConcurrentDictionary<int, IDependencyContainer>();

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

    public IDependencyInjectionRegistration DelegateRegistration(Action<IDependencyInjectionRegistrationContainer> registrationAction)
    {
        return new DependencyInjectionDelegateRegistration(registrationAction);
    }

    public IDependencyContainer DependencyContainer(DependencyContainerOptions options)
    {
        var hash = DependencyContainerHash(options);

        if (Cache.TryGetValue(hash, out var container))
        {
            return container;
        }

        container = new DependencyContainer(options);

        return Cache.AddOrUpdate(hash, _ => container, (_, _) => container);
    }

    private static int DependencyContainerHash(
        DependencyContainerOptions options,
        params Assembly[] aboveAssembly)
    {
        return HashCode.Combine(aboveAssembly.Aggregate(int.MaxValue, HashCode.Combine), options);
    }
}