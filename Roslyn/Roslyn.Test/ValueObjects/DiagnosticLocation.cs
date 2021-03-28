namespace SpaceEngineers.Core.Roslyn.Test.ValueObjects
{
    using System;

    /// <summary>
    /// Location where the diagnostic appears, as determined by source file name, line number, and column number.
    /// </summary>
    public class DiagnosticLocation
    {
        /// <summary> .ctor </summary>
        /// <param name="sourceFile">Name of source file (without extension)</param>
        /// <param name="line">Line</param>
        /// <param name="column">Column</param>
        /// <exception cref="ArgumentOutOfRangeException">Line/Column should be >= -1</exception>
        public DiagnosticLocation(string sourceFile, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line should be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column should be >= -1");
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