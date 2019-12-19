namespace SpaceEngineers.Core.CompositionRoot.Analyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Do not use DependencyContainer.Resolve() / .ResolveCollection() directly,
    ///     because it’s hides complexity and leads to Service Locator anti-pattern.
    /// Instead use auto-wiring via constructor injection.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ContainerRequestAnalyzer : SyntaxAnalyzerBase
    {
        /// <inheritdoc />
        public override string Identifier { get; } = "CR3";

        /// <inheritdoc />
        public override string Title { get; } = "Do not use DependencyContainer.Resolve() / .ResolveCollection() directly, "
                                              + "because it’s hides complexity and leads to Service Locator anti-pattern";

        /// <inheritdoc />
        public override string Message { get; } = "Do not use DependencyContainer.Resolve() / .ResolveCollection() directly, "
                                                + "because it’s hides complexity and leads to Service Locator anti-pattern. "
                                                + "Instead use auto-wiring via constructor injection.";

        /// <inheritdoc />
        public override string Category { get; } = "DI Configuration";

        /// <inheritdoc />
        protected override SyntaxKind SyntaxKind { get; } = SyntaxKind.InvocationExpression;

        /// <inheritdoc />
        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        protected override void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Expression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
            {
                return;
            }

            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            var containerMethods = context.Compilation
                                          .GetTypeByMetadataName(typeof(DependencyContainer).FullName)
                                          .GetMembers()
                                          .OfType<IMethodSymbol>()
                                          .ToArray();

            var condition = context.Compilation
                                   .GetSemanticModel(memberAccess.SyntaxTree)
                                   .GetMemberGroup(memberAccess)
                                   .OfType<IMethodSymbol>()
                                   .Select(z => z.IsGenericMethod ? z.OriginalDefinition : z)
                                   .Any(z => containerMethods.Contains(z));

            if (condition)
            {
                ReportDiagnostic(context, memberAccess.Parent.GetLocation());
            }
        }
    }
}