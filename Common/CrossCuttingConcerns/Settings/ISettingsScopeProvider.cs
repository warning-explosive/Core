namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    /// <summary>
    /// ISettingsScopeProvider
    /// </summary>
    public interface ISettingsScopeProvider
    {
        /// <summary>
        /// Settings scope
        /// </summary>
        public string? Scope { get; }
    }
}