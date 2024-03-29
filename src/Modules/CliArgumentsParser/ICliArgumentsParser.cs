namespace SpaceEngineers.Core.CliArgumentsParser
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Cli arguments parser service
    /// </summary>
    public interface ICliArgumentsParser
    {
        /// <summary>
        /// Parse cli arguments in typed instance
        /// </summary>
        /// <param name="args">Untyped (serialized) cli arguments</param>
        /// <typeparam name="T">Cli args type-argument</typeparam>
        /// <returns>Typed cli args instance</returns>
        T Parse<T>(string[] args)
            where T : class, new();

        /// <summary>
        /// Try parse cli arguments
        /// </summary>
        /// <param name="args">Untyped (serialized) cli arguments</param>
        /// <param name="arguments">Out typed cli args instance</param>
        /// <typeparam name="T">Cli args type-argument</typeparam>
        /// <returns>True - parse success / False - parse failure</returns>
        bool TryParse<T>(string[] args, [NotNullWhen(true)] out T? arguments)
            where T : class, new();
    }
}