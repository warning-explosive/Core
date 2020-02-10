namespace SpaceEngineers.Core.Roslyn.Test.Api
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    public class DiagnosticResult
    {
        private readonly DiagnosticResultLocation[] _locations;

        /// <summary> .ctor </summary>
        /// <param name="id">Identifier</param>
        /// <param name="message">Message</param>
        /// <param name="severity">Diagnostic severity</param>
        /// <param name="locations">Diagnostic result locations</param>
        public DiagnosticResult(string id, string message, DiagnosticSeverity severity, DiagnosticResultLocation[] locations)
        {
            Id = id;
            Message = message;
            Severity = severity;
            _locations = locations;
        }

        /// <summary>
        /// Diagnostic result locations
        /// </summary>
        public IEnumerable<DiagnosticResultLocation> Locations => _locations;

        /// <summary>
        /// Identifier
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Diagnostic severity
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Path
        /// </summary>
        public string Path => _locations.Length > 0 ? _locations[0].SourceFile : string.Empty;

        /// <summary>
        /// Line
        /// </summary>
        public int Line => _locations.Length > 0 ? _locations[0].Line : -1;

        /// <summary>
        /// Column
        /// </summary>
        public int Column => _locations.Length > 0 ? _locations[0].Column : -1;
    }
}
