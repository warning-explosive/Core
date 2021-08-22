namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    internal interface IScopedLifestyleService : IResolvable
    {
        Task DoSmth();
    }
}