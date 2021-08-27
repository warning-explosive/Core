namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class OpenGenericDecorableServiceImpl<T> : IOpenGenericDecorableService<T>
    {
    }
}