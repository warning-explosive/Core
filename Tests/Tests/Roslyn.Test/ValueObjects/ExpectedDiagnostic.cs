namespace SpaceEngineers.Core.Roslyn.Test.ValueObjects
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    public class ExpectedDiagnostic
    {
        private DiagnosticLocation? _location;

        /// <summary> .cctor </summary>
        /// <param name="descriptor">DiagnosticDescriptor</param>
        /// <param name="actualMessage">Message</param>
        /// <param name="severity">Diagnostic severity</param>
        public ExpectedDiagnostic(DiagnosticDescriptor descriptor,
                                  string actualMessage,
                                  DiagnosticSeverity severity)
        {
            Descriptor = descriptor;
            ActualMessage = actualMessage;
            Severity = severity;
        }

        /// <summary>
        /// Expected diagnostic location
        /// </summary>
        public DiagnosticLocation Location => _location ?? throw new InvalidOperationException($"Use {nameof(WithLocation)} before equality comparision");

        /// <summary>
        /// Diagnostic descriptor
        /// </summary>
        public DiagnosticDescriptor Descriptor { get; }

        /// <summary>
        /// Actual message
        /// </summary>
        public string ActualMessage { get; }

        /// <summary>
        /// Diagnostic severity
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// WithLocation
        /// </summary>
        /// <param name="sourceFileName">Source file name (with extension)</param>
        /// <param name="line">line</param>
        /// <param name="column">column</param>
        /// <returns>Expected diagnostic with specified location</returns>
        public ExpectedDiagnostic WithLocation(string sourceFileName, int line, int column)
        {
            var copy = new ExpectedDiagnostic(Descriptor, ActualMessage, DiagnosticSeverity.Error)
                       {
                           _location = new DiagnosticLocation(sourceFileName, line, column)
                       };

            return copy;
        }
    }
}
