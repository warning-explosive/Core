namespace SpaceEngineers.Core.Roslyn.Test.Implementations
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;

    [Component(EnLifestyle.Singleton)]
    internal class AliasSourceTransformer : ISourceTransformer,
                                            ICollectionResolvable<ISourceTransformer>
    {
        public AliasSourceTransformer()
        {
        }

        public SourceText Transform(SourceText source)
        {
            while (TryGetReplacement(source, out var replacement))
            {
                var (content, span) = replacement;
                source = source.Replace(span, content);
            }

            return ReplaceExpectedSuffix(source);
        }

        private static SourceText ReplaceExpectedSuffix(SourceText source)
        {
            return SourceText.From(source.ToString().Replace(Conventions.Expected, string.Empty, StringComparison.Ordinal));
        }

        private static bool TryGetReplacement(SourceText source, out (string, TextSpan) replacement)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var replacements = ((SyntaxNodeOrToken)syntaxTree.GetRoot())
                .Flatten(node => node.ChildNodesAndTokens())
                .SelectMany(node => node.GetLeadingTrivia().Concat(node.GetTrailingTrivia()))
                .Where(trivia => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                .Select(trivia => TryGetAlias(trivia.ToFullString(), out var content)
                    ? (content, trivia.Span)
                    : default)
                .Where(pair => pair != default)
                .ToList();

            if (replacements.Any())
            {
                replacement = replacements.First();
                return true;
            }

            replacement = default;
            return false;
        }

        private static bool TryGetAlias(string comment, [NotNullWhen(true)] out string? content)
        {
            content = ExecutionExtensions
               .Try(comment, GetAlias)
               .Catch<System.Xml.XmlException>()
               .Invoke(_ => default);

            return content != default;
        }

        private static string? GetAlias(string comment)
        {
            var element = XElement.Parse(comment.Trim('/').Trim('*').Trim());

            if (element.Name != Conventions.Analyzer)
            {
                return default;
            }

            var analyzerName = element.Attribute(Conventions.NameAttribute)?.Value;

            return analyzerName.IsNullOrEmpty()
                ? default
                : element.Value;
        }
    }
}