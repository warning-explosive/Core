namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    internal interface IScopedService : IResolvable
    {
        Task DoSmth();
    }
}