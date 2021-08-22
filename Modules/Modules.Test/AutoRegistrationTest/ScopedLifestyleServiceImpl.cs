namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class ScopedLifestyleServiceImpl : IScopedLifestyleService
    {
        public Task DoSmth()
        {
            return Task.Delay(50);
        }
    }
}