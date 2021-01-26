namespace SpaceEngineers.Core.GenericDomain.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// Validation result
    /// </summary>
    public interface IValidationResult
    {
        /// <summary>
        /// Errors
        /// </summary>
        public ICollection<string> Errors { get; }

        /// <summary>
        /// Warnings
        /// </summary>
        public ICollection<string> Warnings { get; }

        /// <summary>
        /// Infos
        /// </summary>
        public ICollection<string> Infos { get; }
    }
}