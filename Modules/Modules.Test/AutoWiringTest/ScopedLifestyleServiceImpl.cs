namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Threading.Tasks;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedLifestyleServiceImpl : IScopedLifestyleService
    {
        public Task DoSmth()
        {
            return Task.Delay(50);
        }
    }
}