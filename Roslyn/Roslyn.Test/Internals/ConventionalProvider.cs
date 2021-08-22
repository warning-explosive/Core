namespace SpaceEngineers.Core.Roslyn.Test.Internals
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Exceptions;
    using Basics.Roslyn;
    using CompositionRoot.Api.Abstractions;
    using Extensions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using ValueObjects;

    /// <summary>
    /// ConventionalProvider
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    internal class ConventionalProvider : IConventionalProvider
    {
        private readonly IDependencyContainer _container;

        private readonly ISourceTransformer _transformer;

        /// <summary> .cctor </summary>
        /// <param name="container">IDependencyContainer</param>
        /// <param name="transformer">ISourceTransformer</param>
        public ConventionalProvider(IDependencyContainer container, ISourceTransformer transformer)
        {
            _container = container;
            _transformer = transformer;
        }

        private static Func<DiagnosticAnalyzer, string> AnalyzerTypeName => analyzer => analyzer.GetType().Name;

        /// <inheritdoc />
        public CodeFixProvider? CodeFixProvider(DiagnosticAnalyzer analyzer)
        {
            var analyzerTypeName = AnalyzerTypeName(analyzer);
            var count = analyzerTypeName.Length - Conventions.Analyzer.Length;
            var codeFixProviderTypeName = analyzerTypeName.Substring(0, count) + Conventions.CodeFix;

            return _container
                  .ResolveCollection<IIdentifiedCodeFix>()
                  .OfType<CodeFixProvider>()
                  .SingleOrDefault(c => c.GetType().Name == codeFixProviderTypeName);
        }

        /// <inheritdoc />
        public IExpectedDiagnosticsProvider ExpectedDiagnosticsProvider(DiagnosticAnalyzer analyzer)
        {
            var analyzerTypeName = AnalyzerTypeName(analyzer);
            var providerTypeName = analyzerTypeName + Conventions.ExpectedDiagnosticsProviderSuffix;

            var providerType = _container
                              .Resolve<ITypeProvider>()
                              .OurTypes
                              .SingleOrDefault(t => t.Name == providerTypeName)
                            ?? throw new NotFoundException($"Provide {nameof(ExpectedDiagnosticsProvider)} for {analyzerTypeName} or place it in directory different from source directory");

            return (IExpectedDiagnosticsProvider)_container.Resolve(providerType);
        }

        /// <inheritdoc />
        public IEnumerable<SourceFile> SourceFiles(DiagnosticAnalyzer analyzer, string? directorySuffix = null)
        {
            return SolutionExtensions
                  .ProjectFile()
                  .Directory
                  .EnsureNotNull($"Project directory {nameof(Roslyn)}.{nameof(Roslyn.Test)} not found")
                  .StepInto(Conventions.SourceDirectory)
                  .StepInto(analyzer.GetType().Name + (directorySuffix ?? string.Empty))
                  .GetFiles("*" + AnalysisExtensions.CSharpDefaultFileExt, SearchOption.TopDirectoryOnly)
                  .Select(file =>
                          {
                              using var stream = file.OpenRead();
                              return (file.NameWithoutExtension(), SourceText.From(stream));
                          })
                  .Select(tuple =>
                          {
                              var (name, text) = tuple;
                              return new SourceFile(name, _transformer.Transform(text));
                          });
        }
    }
}