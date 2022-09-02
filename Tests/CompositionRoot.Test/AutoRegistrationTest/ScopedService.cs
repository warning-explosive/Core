namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using System;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class ScopedService : IScopedService,
                                   IResolvable<IScopedService>
    {
        public Task DoSmth()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }
}