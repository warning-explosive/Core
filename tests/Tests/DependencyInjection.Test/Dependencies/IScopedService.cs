namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

using System.Threading.Tasks;

internal interface IScopedService
{
    Task DoSmth();
}