namespace SpaceEngineers.Core.Web.Api.Containers
{
    /// <summary>
    /// BooleanDataContainer
    /// </summary>
    public class BooleanDataContainer : GenericDataContainer<bool?>
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        public BooleanDataContainer(bool? value)
            : base(EnContainerType.Boolean, value)
        {
        }
    }
}