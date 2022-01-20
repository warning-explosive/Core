namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Generic;
    using Api.Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal class FrameworkDependenciesProvider : IFrameworkDependenciesProvider
    {
        private const string RequireUseDependencyContainerCall = "Call .UseFrameworkDependenciesProvider() during application initialization so as to resolve framework dependencies";

        private IServiceProvider? _serviceProvider;

        private IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException(RequireUseDependencyContainerCall);

        public TService? GetService<TService>()
        {
            return ServiceProvider.GetService<TService>();
        }

        public TService GetRequiredService<TService>()
            where TService : notnull
        {
            return ServiceProvider.GetRequiredService<TService>();
        }

        public IEnumerable<TService> GetServices<TService>()
            where TService : notnull
        {
            return ServiceProvider.GetServices<TService>();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            if (_serviceProvider != null)
            {
                throw new InvalidOperationException("Service provider have already been set");
            }

            _serviceProvider = serviceProvider;
        }
    }
}