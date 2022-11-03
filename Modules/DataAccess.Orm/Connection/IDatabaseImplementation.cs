namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using Api.Exceptions;

    /// <summary>
    /// IDatabaseImplementation
    /// </summary>
    public interface IDatabaseImplementation
    {
        /// <summary>
        /// Handles ORM exception and throws provider dependent exceptions
        /// </summary>
        /// <param name="exception">DatabaseCommandExecutionException</param>
        void Handle(DatabaseCommandExecutionException exception);
    }
}