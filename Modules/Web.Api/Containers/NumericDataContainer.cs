namespace SpaceEngineers.Core.Web.Api.Containers
{
    /// <summary>
    /// NumericDataContainer
    /// </summary>
    public class NumericDataContainer : GenericDataContainer<double?>
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        public NumericDataContainer(double? value)
            : base(EnContainerType.Numeric, value)
        {
        }
    }
}