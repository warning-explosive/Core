namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;

    internal interface IExtraDependencyDecorator : IExtraDependency,
                                                   IDecorator<IExtraDependency>
    {
    }
}