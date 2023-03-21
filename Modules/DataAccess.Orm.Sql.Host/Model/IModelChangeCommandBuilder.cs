namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System.Collections.Generic;
    using Translation;

    /// <summary>
    /// IModelChangeCommandBuilderComposite
    /// </summary>
    public interface IModelChangeCommandBuilderComposite : IModelChangeCommandBuilder
    {
    }

    /// <summary>
    /// IModelChangeCommandBuilder
    /// </summary>
    public interface IModelChangeCommandBuilder
    {
        /// <summary>
        /// Builds model change commands
        /// </summary>
        /// <param name="change">Database model change</param>
        /// <returns>Ongoing operation</returns>
        IEnumerable<ICommand> BuildCommands(IModelChange change);
    }

    /// <summary>
    /// IModelChangeCommandBuilder
    /// </summary>
    /// <typeparam name="TChange">TChange type-argument</typeparam>
    public interface IModelChangeCommandBuilder<TChange> : IModelChangeCommandBuilder
        where TChange : IModelChange
    {
        /// <summary>
        /// Builds model change commands
        /// </summary>
        /// <param name="change">Database model change</param>
        /// <returns>Ongoing operation</returns>
        IEnumerable<ICommand> BuildCommands(TChange change);
    }
}