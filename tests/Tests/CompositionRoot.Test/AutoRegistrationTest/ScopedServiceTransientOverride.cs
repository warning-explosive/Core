namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using System;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class ScopedServiceTransientOverride : IScopedService,
                                                    IResolvable<IScopedService>
    {
        public Task DoSmth()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }
}