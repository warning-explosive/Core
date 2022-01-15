namespace SpaceEngineers.Core.CrossCuttingConcerns.Api.Abstractions
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// ISettingsScopeProvider
    /// </summary>
    public interface ISettingsScopeProvider : IResolvable
    {
        /// <summary>
        /// Settings scope
        /// </summary>
        public string? Scope { get; }
    }
}