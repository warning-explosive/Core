namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Contexts;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
    using AutoWiringTest;
    using Basics;

    internal class VersionedOpenGenericRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            container.RegisterVersioned(typeof(ConcreteImplementationGenericService<object>), EnLifestyle.Transient);
            container.RegisterVersioned(typeof(IComparable<ExternalResolvableImpl>), EnLifestyle.Transient);
        }

        internal static ICollection<Type> RegisterVersionedForOpenGenerics(
            IDependencyContainer donorContainer,
            IRegistrationContainer registrationContainer)
        {
            var typeProvider = donorContainer.Resolve<ITypeProvider>();
            var genericTypeProvider = donorContainer.Resolve<IGenericTypeProvider>();

            return typeProvider
                  .AllLoadedTypes
                  .Where(t => typeof(IResolvable).IsAssignableFrom(t)
                           && t != typeof(IResolvable)
                           && !t.IsSubclassOfOpenGeneric(typeof(IDecorator<>))
                           && t.IsGenericType
                           && !t.IsConstructedGenericType)
                  .Select(service =>
                          {
                              var closed = genericTypeProvider.CloseByConstraints(service, HybridTypeArgumentSelector(donorContainer));
                              var versionedService = typeof(IVersioned<>).MakeGenericType(closed);

                              if (!registrationContainer.HasRegistration(versionedService))
                              {
                                  registrationContainer.RegisterVersioned(closed, EnLifestyle.Transient);
                              }

                              return closed;
                          })
                  .ToList();
        }

        internal static Func<TypeArgumentSelectionContext, Type?> HybridTypeArgumentSelector(IDependencyContainer container)
        {
            return ctx => FromExistedClosedTypesTypeArgumentSelector(container.Resolve<ITypeProvider>().AllLoadedTypes, ctx)
                       ?? FromMatchesTypeArgumentSelector(ctx);
        }

        private static Type? FromExistedClosedTypesTypeArgumentSelector(IEnumerable<Type> source, TypeArgumentSelectionContext ctx)
            => source
              .OrderBy(t => t.IsGenericType)
              .FirstOrDefault(t => t.IsConstructedOrSimpleType() && t.IsSubclassOfOpenGeneric(ctx.OpenGeneric))
             ?.ExtractGenericArgumentsAt(ctx.OpenGeneric, ctx.TypeArgument.GenericParameterPosition)
              .FirstOrDefault();

        private static Type? FromMatchesTypeArgumentSelector(TypeArgumentSelectionContext ctx)
        {
            var isVersioned = ctx.OpenGeneric.IsSubclassOfOpenGeneric(typeof(IVersioned<>));

            var predicate = isVersioned
                                ? type => typeof(IResolvable).IsAssignableFrom(type) && type != typeof(IResolvable)
                                : new Func<Type, bool>(type => true);

            return ctx.Matches.Contains(typeof(object)) && !isVersioned
                       ? typeof(object)
                       : ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault(predicate);
        }
    }
}