namespace SpaceEngineers.Core.Roslyn.Test.Internals
{
    using System;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using ValueObjects;
    using Xunit;

    /// <inheritdoc />
    [Component(EnLifestyle.Singleton)]
    internal class DiagnosticAnalyzerVerifier : IDiagnosticAnalyzerVerifier
    {
        /// <inheritdoc />
        public void VerifyAnalyzedDocument(SyntaxAnalyzerBase analyzer,
                                           AnalyzedDocument analyzedDocument,
                                           ImmutableArray<ExpectedDiagnostic> expectedDiagnostics)
        {
            VerifyDiagnostics(analyzer, analyzedDocument.ActualDiagnostics.ToArray(), expectedDiagnostics.ToArray());
        }

        private static void VerifyDiagnostics(SyntaxAnalyzerBase analyzer,
                                              Diagnostic[] actualDiagnostics,
                                              params ExpectedDiagnostic[] expectedResults)
        {
            var expectedCount = expectedResults.Length;
            var actualCount = actualDiagnostics.Length;

            if (expectedCount != actualCount)
            {
                var diagnosticsOutput = actualDiagnostics.Any() ? FormatDiagnostics(analyzer, actualDiagnostics.ToArray()) : "    NONE.";

                Assert.True(false,
                            $"Mismatch between number of diagnostics returned, expected \"{expectedCount}\" actual \"{actualCount}\"\r\n\r\nDiagnostics:\r\n{diagnosticsOutput}\r\n");
            }

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualDiagnostics[i];
                var expected = expectedResults[i];

                if (expected.Location.Line == -1 && expected.Location.Column == -1)
                {
                    if (actual.Location != Location.None)
                    {
                        Assert.True(false,
                                    $"Expected:\nA project diagnostic with No location\nActual:\n{FormatDiagnostics(analyzer, actual)}");
                    }
                }
                else
                {
                    VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Location);
                }

                if (actual.Id != expected.Descriptor.Id)
                {
                    Assert.True(false,
                                $"Expected diagnostic id to be \"{expected.Descriptor.Id}\" was \"{actual.Id}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, actual)}\r\n");
                }

                if (actual.Severity != expected.Severity)
                {
                    Assert.True(false,
                                $"Expected diagnostic severity to be \"{expected.Severity}\" was \"{actual.Severity}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, actual)}\r\n");
                }

                if (actual.GetMessage() != expected.ActualMessage)
                {
                    Assert.True(false,
                                $"Expected diagnostic message to be \"{expected.ActualMessage}\" was \"{actual.GetMessage()}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, actual)}\r\n");
                }
            }
        }

        private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, DiagnosticLocation expected)
        {
            var actualSpan = actual.GetLineSpan();

            Assert.True(actualSpan.Path == expected.SourceFile
                     || (actualSpan.Path != null
                      && actualSpan.Path.Contains(expected.SourceFile, StringComparison.Ordinal)),
                        $"Expected diagnostic to be in file \"{expected.SourceFile}\" was actually in file \"{actualSpan.Path}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, diagnostic)}\r\n");

            var actualLinePosition = actualSpan.StartLinePosition;

            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0)
            {
                if (actualLinePosition.Line + 1 != expected.Line)
                {
                    Assert.True(false,
                                $"Expected diagnostic to be on line \"{expected.Line}\" was actually on line \"{actualLinePosition.Line + 1}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, diagnostic)}\r\n");
                }
            }

            // Only check column position if there is an actual column position in the real diagnostic
            if (actualLinePosition.Character > 0)
            {
                if (actualLinePosition.Character + 1 != expected.Column)
                {
                    Assert.True(false,
                                $"Expected diagnostic to start at column \"{expected.Column}\" was actually at column \"{actualLinePosition.Character + 1}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, diagnostic)}\r\n");
                }
            }
        }

        private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendLine("// " + diagnostics[i]);

                var analyzerType = analyzer.GetType();
                var rules = analyzer.SupportedDiagnostics;

                foreach (var rule in rules)
                {
                    if (rule != null && rule.Id == diagnostics[i].Id)
                    {
                        var location = diagnostics[i].Location;
                        if (location == Location.None)
                        {
                            builder.AppendFormat(CultureInfo.InvariantCulture, "GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
                        }
                        else
                        {
                            Assert.True(location.IsInSource,
                                $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

                            var resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs", StringComparison.Ordinal)
                                                       ? "expected(("
                                                       : throw new InvalidOperationException("Choose another language from C#");
                            var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                            builder.AppendFormat(CultureInfo.InvariantCulture,
                                "{0}({1}, {2}, {3}.{4}, \"{5}\"))",
                                resultMethodName,
                                linePosition.Line + 1,
                                linePosition.Character + 1,
                                nameof(DiagnosticSeverity),
                                rule.DefaultSeverity,
                                rule.MessageFormat);
                        }

                        if (i != diagnostics.Length - 1)
                        {
                            builder.Append(',');
                        }

                        builder.AppendLine();
                        break;
                    }
                }
            }

            return builder.ToString();
        }
    }
}
