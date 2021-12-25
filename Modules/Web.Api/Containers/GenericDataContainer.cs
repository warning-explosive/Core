namespace SpaceEngineers.Core.Web.Api.Containers
{
    /// <summary>
    /// GenericDataContainer
    /// </summary>
    /// <typeparam name="TValue">TValue type-argument</typeparam>
    public abstract class GenericDataContainer<TValue> : IDataContainer
    {
        /// <summary> .cctor </summary>
        /// <param name="containerType">Container type</param>
        /// <param name="value">Value</param>
        protected GenericDataContainer(
            EnContainerType containerType,
            TValue value)
        {
            ContainerType = containerType;
            Value = value;
        }

        /// <summary>
        /// Container type
        /// </summary>
        public EnContainerType ContainerType { get; }

        /// <summary>
        /// Value
        /// </summary>
        public TValue Value { get; }
    }
}