namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    internal interface IScopedLifestyleService : IResolvable
    {
        Task DoSmth();
    }
}