namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System.Collections.Generic;

    public interface ICollectionResolvableConditionDecorableServiceDecorator : ICollectionResolvableConditionDecorableService
    {
        ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}