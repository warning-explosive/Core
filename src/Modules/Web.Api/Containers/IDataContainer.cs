namespace SpaceEngineers.Core.Web.Api.Containers
{
    /// <summary>
    /// IDataContainer
    /// </summary>
    public interface IDataContainer
    {
        /// <summary>
        /// Container type
        /// </summary>
        EnContainerType ContainerType { get; }
    }
}