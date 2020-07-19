namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;

    internal interface IServiceForInterceptionDecorator : IServiceForInterception,
                                                          IDecorator<IServiceForInterception>
    {
    }
}