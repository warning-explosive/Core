namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class VersionedAndDecoratedDecorator : IVersionedAndDecoratedDecorator
    {
        public VersionedAndDecoratedDecorator(IVersionedAndDecorated decoratee)
        {
            Decoratee = decoratee;
        }

        public IVersionedAndDecorated Decoratee { get; }
    }
}