namespace SpaceEngineers.Core.Roslyn.Test.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class AliasSourceTransformer : ISourceTransformer
    {
        /// <summary> .cctor </summary>
        public AliasSourceTransformer()
        {
        }

        /// <inheritdoc />
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
            var replacements = Flatten(syntaxTree.GetRoot(), node => node.ChildNodesAndTokens())
                              .SelectMany(node => node.GetLeadingTrivia().Concat(node.GetTrailingTrivia()))
                              .Where(trivia => trivia.Kind() == SyntaxKind.MultiLineCommentTrivia)
                              .Select(trivia =>
                                      {
                                          if (TryGetAlias(trivia.ToFullString(), out var content))
                                          {
                                              return (content, trivia.Span);
                                          }

                                          return default;
                                      })
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

        private static IEnumerable<SyntaxNodeOrToken> Flatten(SyntaxNodeOrToken nodeOrToken, Func<SyntaxNodeOrToken, IEnumerable<SyntaxNodeOrToken>> unfold)
        {
            return new[] { nodeOrToken }.Concat(unfold(nodeOrToken).Flatten(unfold));
        }

        private static bool TryGetAlias(string comment, out string content)
        {
            content = string.Empty;

            try
            {
                var element = XElement.Parse(comment.Trim('/').Trim('*').Trim());

                if (element.Name != Conventions.Analyzer)
                {
                    return false;
                }

                var analyzerName = element.Attribute(Conventions.NameAttribute)?.Value;

                if (string.IsNullOrEmpty(analyzerName))
                {
                    return false;
                }

                content = element.Value;
                return true;
            }
            catch (System.Xml.XmlException)
            {
                return false;
            }
        }
    }
}