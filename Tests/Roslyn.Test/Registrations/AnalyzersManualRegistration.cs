namespace SpaceEngineers.Core.Roslyn.Test.Registrations
{
    using System.Linq;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

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