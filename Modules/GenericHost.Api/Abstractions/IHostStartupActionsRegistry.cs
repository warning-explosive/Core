namespace SpaceEngineers.Core.GenericHost.Api.Abstractions
{
    /// <summary>
    /// IHostStartupActionsRegistry
    /// </summary>
    public interface IHostStartupActionsRegistry
    {
        /// <summary>
        /// Enrolls specified host startup action
        /// </summary>
        /// <param name="action">IHostStartupAction</param>
        void Enroll(IHostStartupAction action);

        /// <summary>
        /// Returns true if startup action was previously enrolled
        /// </summary>
        /// <param name="action">IHostStartupAction</param>
        /// <returns>True if startup action was previously enrolled</returns>
        bool Contains(IHostStartupAction action);
    }
}