namespace SpaceEngineers.Core.Roslyn.Test.ManualRegistrations
{
    using System.Linq;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class AnalyzersManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var analyzers = container
                .Types
                .OurTypes
                .Where(type => typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && type.IsConcreteType())
                .ToList();

            container.RegisterCollection(typeof(DiagnosticAnalyzer), analyzers, EnLifestyle.Singleton);

            var codeFixes = container
                .Types
                .OurTypes
                .Where(type => typeof(CodeFixProvider).IsAssignableFrom(type) && type.IsConcreteType())
                .ToList();

            container.RegisterCollection(typeof(CodeFixProvider), codeFixes, EnLifestyle.Singleton);
        }
    }
}