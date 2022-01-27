namespace SpaceEngineers.Core.Roslyn.Test.ManualRegistrations
{
    using System.Linq;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Registration;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class AnalyzersManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container
                .Types
                .OurTypes
                .Where(type => typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && type.IsConcreteType())
                .Each(implementation => container.RegisterCollectionEntry<DiagnosticAnalyzer>(implementation, EnLifestyle.Singleton));

            container
                .Types
                .OurTypes
                .Where(type => typeof(CodeFixProvider).IsAssignableFrom(type) && type.IsConcreteType())
                .Each(implementation => container.RegisterCollectionEntry<CodeFixProvider>(implementation, EnLifestyle.Singleton));
        }
    }
}