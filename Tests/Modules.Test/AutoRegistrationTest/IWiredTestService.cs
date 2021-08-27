namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface IWiredTestService : IResolvable
    {
        IIndependentTestService IndependentTestService { get; }
    }
}