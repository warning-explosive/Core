namespace SpaceEngineers.Core.CompositionRoot.Analyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Attributes;
    using Enumerations;
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
        private const string EnLifestyleValue = "ChooseLifestyle";

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
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

            if (root == null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First(z => FixableDiagnosticIds.Contains(z.Id));

            var syntaxAnnotation = new SyntaxAnnotation();

            /*
             * Mark NamespaceDeclarationSyntax by SyntaxAnnotation
             */
            root = MarkNode(root,
                            GetSyntax<NamespaceDeclarationSyntax>(root, diagnostic),
                            syntaxAnnotation);

            /*
             * Mark TypeDeclarationSyntax by SyntaxAnnotation
             */
            root = MarkNode(root,
                            GetSyntax<TypeDeclarationSyntax>(root, diagnostic),
                            syntaxAnnotation);

            /*
             * AddUsingDirective
             */
            root = AddUsingDirective(root, FindMarked<NamespaceDeclarationSyntax>(root, syntaxAnnotation));

            /*
             * InsertAttribute
             */
            root = InsertAttribute(root, FindMarked<TypeDeclarationSyntax>(root, syntaxAnnotation));

            /*
             * Register a code action that will invoke the fix.
             */
            var codeAction = CodeAction.Create(Title,
                                               c => Task.FromResult(context.Document.WithSyntaxRoot(root).Project.Solution),
                                               Title);

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static TSyntax GetSyntax<TSyntax>(SyntaxNode root, Diagnostic diagnostic)
            where TSyntax : SyntaxNode
        {
            return root
                  .FindToken(diagnostic.Location.SourceSpan.Start)
                  .Parent
                  .AncestorsAndSelf()
                  .OfType<TSyntax>()
                  .First();
        }

        private static TSyntax FindMarked<TSyntax>(SyntaxNode root, SyntaxAnnotation syntaxAnnotation)
            where TSyntax : SyntaxNode
        {
            return root.DescendantNodes()
                       .OfType<TSyntax>()
                       .Single(n => n.HasAnnotation(syntaxAnnotation));
        }

        private static SyntaxNode MarkNode<TSyntax>(SyntaxNode root,
                                                    TSyntax syntax,
                                                    SyntaxAnnotation syntaxAnnotation)
            where TSyntax : SyntaxNode
        {
            return root.ReplaceNode(syntax,
                                    syntax.WithAdditionalAnnotations(syntaxAnnotation));
        }

        private SyntaxNode AddUsingDirective(SyntaxNode root, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            var trivia = namespaceDeclaration.Usings
                                             .First()
                                             .GetLeadingTrivia();

            var usingDirective = SyntaxFactory
                                .UsingDirective(SyntaxFactory.IdentifierName(typeof(LifestyleAttribute).Namespace))
                                .NormalizeWhitespace()
                                .WithLeadingTrivia(trivia)
                                .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));

            return root.ReplaceNode(namespaceDeclaration,
                                    namespaceDeclaration.AddUsings(usingDirective));
        }

        private SyntaxNode InsertAttribute(SyntaxNode root, TypeDeclarationSyntax typeDeclaration)
        {
            var originalAttributesList = typeDeclaration.AttributeLists;

            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName(nameof(EnLifestyle)),
                                                                    SyntaxFactory.IdentifierName(EnLifestyleValue));

            var argument = SyntaxFactory.AttributeArgument(memberAccess);

            var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

            var name = nameof(LifestyleAttribute).Substring(0, nameof(LifestyleAttribute).Length - nameof(Attribute).Length);

            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(name))
                                         .WithArgumentList(SyntaxFactory.AttributeArgumentList(argumentList));

            var additionalAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                                                   .NormalizeWhitespace()
                                                   .WithLeadingTrivia(typeDeclaration.GetLeadingTrivia());

            var extendedAttributeList = originalAttributesList.Insert(0, additionalAttribute);

            return root.ReplaceNode(typeDeclaration,
                                    typeDeclaration.WithAttributeLists(extendedAttributeList));
        }
    }
}
