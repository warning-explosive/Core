namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using CompositionRoot.Api.Abstractions;

    internal static class TypeProviderExtensions
    {
        internal static ITypeProvider ExtendTypeProvider(this ITypeProvider decoratee, params Type[] additionalOurTypes)
        {
            return new ExtendedTypeProviderDecorator(
                decoratee,
                new ExtendedTypeProviderDecorator.TypeProviderExtension(additionalOurTypes));
        }
    }
}