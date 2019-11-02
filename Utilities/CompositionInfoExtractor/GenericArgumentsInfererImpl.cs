namespace SpaceEngineers.Core.Utilities.CompositionInfoExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CompositionRoot.Abstractions;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using CompositionRoot.Extensions;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class GenericArgumentsInfererImpl : IGenericArgumentsInferer
    {
        public Type CloseByConstraints(Type type)
        {
            if (TryCloseByConstraints(type, out var closedOrSameType))
            {
                return closedOrSameType;
            }

            throw new ArgumentException($"Type {type.FullName} is not closed");
        }
        
        private bool TryCloseByConstraints(Type type, out Type closedOrSameType)
        {
            closedOrSameType = type;
            
            if (AlreadyClosed(type))
            {
                return true;
            }

            var args = type.GetGenericArguments()
                           .Select(arg =>
                                   {
                                       var inferred = InferType(arg.GetGenericParameterConstraints(), arg.GenericParameterAttributes);

                                       return CloseByConstraints(inferred);
                                   })
                           .ToArray();

            closedOrSameType = type.MakeGenericType(args);
            
            if (AlreadyClosed(closedOrSameType))
            {
                return true;
            }

            return false;
        }

        private static bool AlreadyClosed(Type type)
        {
            return !type.IsGenericType
                   || type.IsConstructedGenericType;
        }
        
        private Type InferType(Type[] constraints, GenericParameterAttributes genericParameterAttributes)
        {
            var filters = GetFiltersByTypeParameterAttributes(genericParameterAttributes);

            bool CheckTypeArgument(Type type) => CheckTypeArgumentInternal(type, constraints, filters);

            return TypeExtensions.OurTypes()
                                 .FirstOrDefault(CheckTypeArgument)
                   ?? TypeExtensions.AllLoadedTypes()
                                    .First(CheckTypeArgument);
        }

        private static bool CheckTypeArgumentInternal(Type typeArgument,
                                                      Type[] constraints,
                                                      ICollection<Func<Type, bool>> filters)
        {
            return constraints.All(c => !c.IsGenericType
                                            ? c.IsAssignableFrom(typeArgument)
                                            : c.IsInterface
                                                ? typeArgument.IsImplementationOfOpenGenericInterface(c.GetGenericTypeDefinition())
                                                : typeArgument.IsImplementationOfOpenGeneric(c.GetGenericTypeDefinition()))
                   && filters.All(f => f(typeArgument));
        }

        private static ICollection<Func<Type, bool>> GetFiltersByTypeParameterAttributes(GenericParameterAttributes genericParameterAttributes)
        {
            var filters = new List<Func<Type, bool>>();
            
            if ((genericParameterAttributes & GenericParameterAttributes.None) != 0)
            {
                filters.Add(type => true);

                return filters;
            }

            if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                filters.Add(type => type.IsClass);
            }

            if ((genericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                filters.Add(type => type.IsValueType);
            }

            if ((genericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
                filters.Add(type =>
                            {
                                if (type.IsEnum)
                                {
                                    return true;
                                }
                                
                                var ctor = type.GetConstructor(Array.Empty<Type>());
                                
                                return ctor != null;
                            });
            }

            return filters;
        }
    }
}