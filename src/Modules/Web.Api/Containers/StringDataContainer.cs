namespace SpaceEngineers.Core.Web.Api.Containers
{
    /// <summary>
    /// StringDataContainer
    /// </summary>
    public class StringDataContainer : GenericDataContainer<string?>
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        public StringDataContainer(string? value)
            : base(EnContainerType.String, value)
        {
        }
    }
}