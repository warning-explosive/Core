namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class GenericTypeProvider : IGenericTypeProvider,
                                         IResolvable<IGenericTypeProvider>
    {
        private readonly ITypeProvider _typeProvider;

        public GenericTypeProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IEnumerable<Type> AllSatisfyingTypesAt(Type openGeneric, int typeArgumentAt = 0)
        {
            if (!openGeneric.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Should be GenericTypeDefinition", nameof(openGeneric));
            }

            if (typeArgumentAt < 0 || typeArgumentAt >= openGeneric.GetGenericArguments().Length)
            {
                throw new ArgumentException("Should be in bounds of generic arguments count", nameof(typeArgumentAt));
            }

            var typeArgument = openGeneric.GetGenericArguments()[typeArgumentAt];

            return _typeProvider.AllLoadedTypes.Where(t => t.FitsForTypeArgument(typeArgument));
        }

        public Type CloseByConstraints(Type type, Func<TypeArgumentSelectionContext, Type?>? selector)
        {
            return type.IsConstructedOrNonGenericType()
                ? type
                : CloseByConstraintsInternal(type, selector ?? DefaultSelector());
        }

        public Func<TypeArgumentSelectionContext, Type?> DefaultSelector()
        {
            return ctx =>
            {
                return ctx.Matches
                          .OrderBy(t => t.IsGenericType)
                          .InformativeSingleOrDefault(Amb, ctx);

                static string Amb(TypeArgumentSelectionContext ctx, IEnumerable<Type> source)
                {
                    return "Type:"
                           + ctx.OpenGeneric
                           + Environment.NewLine
                           + "Candidates:"
                           + Environment.NewLine
                           + source.Take(10).Select(t => t.FullName).ToString(Environment.NewLine)
                           + Environment.NewLine
                           + "...";
                }
            };
        }

        private Type CloseByConstraintsInternal(Type type, Func<TypeArgumentSelectionContext, Type?> selector)
        {
            var resolved = new List<Type>();

            var argsCount = type
                .GetGenericTypeDefinition()
                .GetGenericArguments()
                .Length;

            for (var at = 0; at < argsCount; at++)
            {
                var typeArgument = CloseByConstraints(SingleSatisfyingType(type, at, selector, resolved), selector);
                resolved.Add(typeArgument);
            }

            var closed = type.MakeGenericType(resolved.ToArray());

            if (!closed.IsConstructedOrNonGenericType())
            {
                throw new ArgumentException($"Type {type.FullName} is not closed");
            }

            return closed;
        }

        private Type SingleSatisfyingType(
            Type genericType,
            int at,
            Func<TypeArgumentSelectionContext, Type?> selector,
            IReadOnlyCollection<Type> resolved)
        {
            var matches = AllSatisfyingTypesAt(genericType, at).ToList();
            var typeArgument = genericType.GetGenericArguments()[at];
            var selectionContext = new TypeArgumentSelectionContext(genericType, typeArgument, matches, resolved);

            return selector(selectionContext)
                   ?? throw new InvalidOperationException($"Satisfying type for type argument {typeArgument} in {genericType} wasn't found");
        }
    }
}