namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class OpenGenericTestService<T> : IOpenGenericTestService<T>,
                                               IResolvable<IOpenGenericTestService<T>>
    {
    }
}