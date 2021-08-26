namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class GenericTypeProvider : IGenericTypeProvider
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
            return AlreadyClosed(type)
                       ? type
                       : CloseByConstraintsInternal(type, selector ?? DefaultSelector());
        }

        public Func<TypeArgumentSelectionContext, Type?> DefaultSelector()
        {
            return ctx =>
            {
                return ctx.Matches
                          .OrderBy(t => t.IsGenericType)
                          .InformativeSingleOrDefault(Amb);

                string Amb(IEnumerable<Type> source)
                {
                    return "Type:"
                           + ctx.OpenGeneric
                           + Environment.NewLine
                           + "Candidates:"
                           + Environment.NewLine
                           + string.Join(Environment.NewLine, source.Take(10).Select(t => t.FullName))
                           + Environment.NewLine
                           + "...";
                }
            };
        }

        private Type CloseByConstraintsInternal(Type type, Func<TypeArgumentSelectionContext, Type?> selector)
        {
            var args = type.GetGenericTypeDefinition()
                           .GetGenericArguments()
                           .Select((typeArgument, at) => CloseByConstraints(SingleSatisfyingType(type, at, selector), selector))
                           .ToArray();

            var closed = type.MakeGenericType(args);

            if (!AlreadyClosed(closed))
            {
                throw new ArgumentException($"Type {type.FullName} is not closed");
            }

            return closed;
        }

        private static bool AlreadyClosed(Type type)
        {
            return !type.IsGenericType
                   || type.IsConstructedGenericType;
        }

        private Type SingleSatisfyingType(Type genericType, int at, Func<TypeArgumentSelectionContext, Type?> selector)
        {
            var matches = AllSatisfyingTypesAt(genericType, at);
            var typeArgument = genericType.GetGenericArguments()[at];
            var selectionContext = new TypeArgumentSelectionContext(genericType, typeArgument, matches);

            return selector(selectionContext).EnsureNotNull($"Satisfying type for type argument {typeArgument} in {genericType} not found");
        }
    }
}