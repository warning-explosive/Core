namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Abstractions
{
    /// <summary>
    /// IUserScopeProvider
    /// </summary>
    public interface IUserScopeProvider
    {
        /// <summary>
        /// Tries to get user from ambient context
        /// </summary>
        /// <param name="initiatorMessage">Initiator message</param>
        /// <param name="user">User</param>
        /// <returns>True if user extracted successfully</returns>
        bool TryGetUser(IntegrationMessage? initiatorMessage, out string? user);
    }
}