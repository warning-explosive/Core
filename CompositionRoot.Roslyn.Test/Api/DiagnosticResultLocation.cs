namespace SpaceEngineers.Core.CompositionRoot.Roslyn.Test.Api
{
    using System;

    /// <summary>
    /// Location where the diagnostic appears, as determined by source file name, line number, and column number.
    /// </summary>
    public class DiagnosticResultLocation
    {
        /// <summary> .ctor </summary>
        /// <param name="sourceFile">Name of source file (without extension)</param>
        /// <param name="line">Line</param>
        /// <param name="column">Column</param>
        /// <exception cref="ArgumentOutOfRangeException">Line/Column must be >= -1</exception>
        public DiagnosticResultLocation(string sourceFile, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            }

            SourceFile = sourceFile;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Name of source file (without extension)
        /// </summary>
        public string SourceFile { get; }

        /// <summary>
        /// Line
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Column
        /// </summary>
        public int Column { get; }
    }
}