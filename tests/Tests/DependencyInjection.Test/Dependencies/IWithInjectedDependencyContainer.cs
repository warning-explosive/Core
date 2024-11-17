namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

internal interface IWithInjectedDependencyContainer
{
    IDependencyContainer DependencyContainer { get; }
}