namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;

    internal interface IScopedLifestyleService : IResolvable
    {
        Task DoSmth();
    }
}