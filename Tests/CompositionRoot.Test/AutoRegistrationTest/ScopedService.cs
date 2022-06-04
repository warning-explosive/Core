namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using System;
    using System.Threading.Tasks;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

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