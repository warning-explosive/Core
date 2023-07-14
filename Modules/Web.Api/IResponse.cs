namespace SpaceEngineers.Core.Web.Api
{
    using System.Collections.Generic;

    /// <summary>
    /// IResponse
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Success
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Errors
        /// </summary>
        IReadOnlyCollection<Error> Errors { get; }
    }
}