namespace SpaceEngineers.Core.Web.Api.Containers
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// EnContainerType
    /// </summary>
    public enum EnContainerType
    {
        /// <summary>
        /// String
        /// </summary>
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        String = 0,

        /// <summary>
        /// Numeric
        /// </summary>
        Numeric = 1,

        /// <summary>
        /// DateTime
        /// </summary>
        DateTime = 2,

        /// <summary>
        /// Boolean
        /// </summary>
        Boolean = 3
    }
}