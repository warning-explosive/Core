namespace SpaceEngineers.Core.Roslyn.Test.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Exceptions;
    using CompositionRoot;
    using Extensions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using ValueObjects;

    /// <summary>
    /// ConventionalProvider
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    internal class ConventionalProvider : IConventionalProvider,
                                          IResolvable<IConventionalProvider>
    {
        private readonly IDependencyContainer _dependencyContainer;

        private readonly IEnumerable<ISourceTransformer> _transformers;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="transformers">ISourceTransformer</param>
        public ConventionalProvider(IDependencyContainer dependencyContainer, IEnumerable<ISourceTransformer> transformers)
        {
            _dependencyContainer = dependencyContainer;
            _transformers = transformers;
        }

        private static Func<DiagnosticAnalyzer, string> AnalyzerTypeName => analyzer => analyzer.GetType().Name;

        /// <inheritdoc />
        public CodeFixProvider? CodeFixProvider(DiagnosticAnalyzer analyzer)
        {
            var analyzerTypeName = AnalyzerTypeName(analyzer);
            var count = analyzerTypeName.Length - Conventions.Analyzer.Length;
            var codeFixProviderTypeName = analyzerTypeName.Substring(0, count) + Conventions.CodeFix;

            return _dependencyContainer
                  .ResolveCollection<CodeFixProvider>()
                  .SingleOrDefault(c => c.GetType().Name == codeFixProviderTypeName);
        }

        /// <inheritdoc />
        public IExpectedDiagnosticsProvider ExpectedDiagnosticsProvider(DiagnosticAnalyzer analyzer)
        {
            var analyzerTypeName = AnalyzerTypeName(analyzer);
            var providerTypeName = analyzerTypeName + Conventions.ExpectedDiagnosticsProviderSuffix;

            var providerType = _dependencyContainer
                .Resolve<ITypeProvider>()
                .OurTypes
                .SingleOrDefault(t => t.Name == providerTypeName)
                .EnsureNotNull(() => new NotFoundException($"Provide {nameof(ExpectedDiagnosticsProvider)} for {analyzerTypeName} or place it in directory different from source directory"));

            return (IExpectedDiagnosticsProvider)_dependencyContainer.Resolve(providerType);
        }

        /// <inheritdoc />
        public IEnumerable<SourceFile> SourceFiles(DiagnosticAnalyzer analyzer, string? directorySuffix = null)
        {
            return SolutionExtensions
                  .ProjectFile()
                  .Directory
                  .EnsureNotNull($"Project directory {nameof(Roslyn)}.{nameof(Test)} wasn't found")
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
                              return new SourceFile(name, _transformers.Aggregate(text, (acc, next) => next.Transform(acc)));
                          });
        }
    }
}