namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class ScopedServiceOverride : IScopedService
    {
        public Task DoSmth()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }
}