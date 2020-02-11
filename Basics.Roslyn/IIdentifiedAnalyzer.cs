namespace SpaceEngineers.Core.Basics.Roslyn
{
    /// <summary>
    /// IIdentifiedAnalyzer
    /// </summary>
    public interface IIdentifiedAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Diagnostic title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Diagnostic error message
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Diagnostic category
        /// </summary>
        string Category { get; }
    }
}