namespace SpaceEngineers.Core.CompositionRoot.Analyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// CodeFixProvider that inserts LifestyleAttribute on components (component - service implementation)
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LifestyleAttributeCodeFixProvider))]
    public class LifestyleAttributeCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Mark with LifestyleAttribute";

        /// <inheritdoc />
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(LifestyleAttributeAnalyzer.DiagnosticDescriptor.Id);

        /// <inheritdoc />
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc />
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            /*
             * 1. Find the type declaration identified by the diagnostic.
             */
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root == null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First(z => FixableDiagnosticIds.Contains(z.Id));

            var typeDeclaration = root
                                 .FindToken(diagnostic.Location.SourceSpan.Start)
                                 .Parent
                                 .AncestorsAndSelf()
                                 .OfType<TypeDeclarationSyntax>()
                                 .First();

            /*
             * 2. Register a code action that will invoke the fix.
             */
            context.RegisterCodeFix(CodeAction.Create(Title,
                                                      c => InsertAttribute(root, context.Document, typeDeclaration, c),
                                                      Title),
                                    diagnostic);
        }

        private Task<Solution> InsertAttribute(SyntaxNode root, Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken token)
        {
            var originalAttributesList = typeDeclaration.AttributeLists;

            var argument = SyntaxFactory.AttributeArgument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                                SyntaxFactory.IdentifierName("EnLifeStyle"),
                                                                                                SyntaxFactory.IdentifierName("ChooseLifestyle")));

            var argumentList = SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(new[] { argument });

            var additionalAttribute = SyntaxFactory.SingletonSeparatedList(SyntaxFactory
                                                                          .Attribute(SyntaxFactory.IdentifierName("LifestyleAttribute"))
                                                                          .WithArgumentList(SyntaxFactory.AttributeArgumentList(argumentList)));

            var extendedAttributeList = originalAttributesList.Add(SyntaxFactory.AttributeList(additionalAttribute)
                                                                                .NormalizeWhitespace());

            return Task.Factory.StartNew(ReplaceNode, token, TaskCreationOptions.None, TaskScheduler.Current);

            Solution ReplaceNode() => document.WithSyntaxRoot(root.ReplaceNode(typeDeclaration,
                                                                               typeDeclaration.WithAttributeLists(extendedAttributeList)))
                                              .Project
                                              .Solution;
        }
    }
}
