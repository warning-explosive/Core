namespace SpaceEngineers.Core.Roslyn.Test.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using ValueObjects;
    using Xunit;

    [Component(EnLifestyle.Singleton)]
    internal class CodeFixVerifier : ICodeFixVerifier,
                                     IResolvable<ICodeFixVerifier>
    {
        public async Task VerifyCodeFix(DiagnosticAnalyzer analyzer,
                                        CodeFixProvider codeFix,
                                        AnalyzedDocument analyzedDocument,
                                        SourceFile expectedSource,
                                        Action<string> show)
        {
            var fixedDocument = await ApplyFix(codeFix, analyzedDocument.Document, analyzedDocument.ActualDiagnostics).ConfigureAwait(false);
            var actualSource = await fixedDocument.GetTextAsync().ConfigureAwait(false);

            if (expectedSource.Text.ToString() != actualSource.ToString())
            {
                show("Expected: " + expectedSource.Text);
                show("Actual: " + actualSource);
            }

            Assert.Equal(expectedSource.Text.ToString(), actualSource.ToString());
        }

        private static async Task<Document> ApplyFix(
            CodeFixProvider codeFix,
            Document document,
            ImmutableArray<Diagnostic> actualDiagnostics)
        {
            var actions = new List<CodeAction>();

            foreach (var diagnostic in actualDiagnostics)
            {
                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (a, d) => actions.Add(a),
                    CancellationToken.None);

                await codeFix
                   .RegisterCodeFixesAsync(context)
                   .ConfigureAwait(false);
            }

            foreach (var codeAction in actions)
            {
                document = (await codeAction
                               .GetOperationsAsync(CancellationToken.None)
                               .ConfigureAwait(false))
                           .OfType<ApplyChangesOperation>()
                           .Single()
                           .ChangedSolution
                           .GetDocument(document.Id)
                           ?? throw new InvalidOperationException($"{nameof(ApplyChangesOperation.ChangedSolution)} must contains document {document.Id}");
            }

            return document;
        }
    }
}
