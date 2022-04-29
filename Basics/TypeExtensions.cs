namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Exceptions;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Is object an instance of specified type
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="type">Type</param>
        /// <returns>Object is an instance of specified type or not</returns>
        public static bool IsInstanceOfType(this object obj, Type type)
        {
            return type.IsInstanceOfType(obj);
        }

        /// <summary>
        /// Returns an attribute that type is anonymous or not
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Whether type is anonymous or not</returns>
        public static bool IsAnonymous(this Type type)
        {
            return type.HasAttribute<CompilerGeneratedAttribute>();
        }

        /// <summary>
        /// Gets default value of the specified system type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Default value</returns>
        public static object? DefaultValue(this Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        /// <summary>
        /// Is type collection or not
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is type can be interpreted as collection</returns>
        public static bool IsCollection(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// Is type primitive or not
        /// - Boolean,
        /// - Byte
        /// - SByte
        /// - Int16
        /// - UInt16
        /// - Int32
        /// - UInt32
        /// - Int64
        /// - UInt64
        /// - IntPtr
        /// - UIntPtr
        /// - Char
        /// - Double
        /// - Single
        /// - Decimal
        /// - Enum
        /// - Guid
        /// - DateTime
        /// - TimeSpan
        /// - string
        /// - System.Type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is type can be interpreted as primitive</returns>
        public static bool IsPrimitive(this Type type)
        {
            return type.IsNullable()
                ? IsPrimitiveType(type.UnwrapTypeParameter(typeof(Nullable<>)))
                : IsPrimitiveType(type);

            static bool IsPrimitiveType(Type t)
            {
                return t.IsPrimitive
                       || t.IsEnum
                       || t == typeof(Guid)
                       || t == typeof(DateTime)
                       || t == typeof(TimeSpan)
                       || t == typeof(decimal)
                       || t == typeof(string)
                       || t == typeof(Type);
            }
        }

        /// <summary>
        /// Is type numeric or not
        /// - Int16
        /// - UInt16
        /// - Int32
        /// - UInt32
        /// - Int64
        /// - UInt64
        /// - Double
        /// - Single
        /// - Decimal
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Result of check</returns>
        public static bool IsNumeric(this Type type)
        {
            return type.IsNullable()
                ? IsNumericType(type.UnwrapTypeParameter(typeof(Nullable<>)))
                : IsNumericType(type);

            static bool IsNumericType(Type t)
            {
                return t == typeof(short)
                       || t == typeof(ushort)
                       || t == typeof(int)
                       || t == typeof(uint)
                       || t == typeof(long)
                       || t == typeof(ulong)
                       || t == typeof(float)
                       || t == typeof(double)
                       || t == typeof(decimal);
            }
        }

        /// <summary>
        /// Does type implement Nullable
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <returns>Result of check</returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// True for non-generic or constructed generic types
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is constructed or simple type</returns>
        public static bool IsConstructedOrNonGenericType(this Type type)
        {
            return !type.IsGenericType || type.IsConstructedGenericType;
        }

        /// <summary>
        /// Is type generic and partially closed
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type is generic and partially closed or not</returns>
        public static bool IsPartiallyClosed(this Type type)
        {
            return type.IsGenericType
                   && type.ContainsGenericParameters
                   && type.GetGenericTypeDefinition() != type;
        }

        /// <summary>
        /// Is type concrete
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type is concrete</returns>
        public static bool IsConcreteType(this Type type)
        {
            return !type.IsAbstract
                   && !type.IsArray
                   && type != typeof(object)
                   && !typeof(Delegate).IsAssignableFrom(type);
        }

        /// <summary>
        /// Is constructor decorates specified type
        /// </summary>
        /// <param name="cctor">Constructor</param>
        /// <param name="dependency">Decorator dependency type</param>
        /// <returns>Constructor decorates specified type</returns>
        public static bool IsDecorator(this ConstructorInfo cctor, Type dependency)
        {
            return cctor
                .SelfDependencies(dependency)
                .Any(type => ContainsDecorateeParameters(cctor, type));

            static bool ContainsDecorateeParameters(ConstructorInfo cctor, Type dependency)
            {
                return cctor
                    .GetParameters()
                    .Any(parameter => IsDecorateeParameter(parameter, dependency));
            }

            static bool IsDecorateeParameter(ParameterInfo parameter, Type dependency)
            {
                return parameter.ParameterType == dependency;
            }
        }

        /// <summary>
        /// Is constructor composes collections of specified type
        /// </summary>
        /// <param name="cctor">Constructor</param>
        /// <param name="dependency">Composite dependency type</param>
        /// <returns>Constructor composes collections of specified type</returns>
        public static bool IsComposite(this ConstructorInfo cctor, Type dependency)
        {
            return cctor
                .SelfDependencies(dependency)
                .Any(type => ContainsCompositeParameters(cctor, type));

            static bool ContainsCompositeParameters(ConstructorInfo cctor, Type dependency)
            {
                return cctor
                    .GetParameters()
                    .Any(parameter => IsCompositeParameter(parameter, dependency));
            }

            static bool IsCompositeParameter(ParameterInfo parameter, Type dependency)
            {
                return parameter
                    .ParameterType
                    .UnwrapTypeParameter(typeof(IEnumerable<>)) == dependency;
            }
        }

        /// <summary>
        /// Does type derived from open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericAncestor">Open generic ancestor</param>
        /// <returns>Result of check</returns>
        public static bool IsSubclassOfOpenGeneric(this Type type, Type openGenericAncestor)
        {
            if (!openGenericAncestor.IsGenericTypeDefinition)
            {
                return false;
            }

            return TypeInfoStorage.Get(type).GenericTypeDefinitions.Contains(openGenericAncestor)
                   || TypeInfoStorage.Get(type).GenericInterfaceDefinitions.Contains(openGenericAncestor);
        }

        /// <summary>
        /// Does type contains interface declaration
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="interface">Type-candidate for interface declaration</param>
        /// <returns>Result of check</returns>
        public static bool IsContainsInterfaceDeclaration(this Type type, Type @interface)
        {
            if (TypeInfoStorage.Get(type).DeclaredInterfaces.Contains(@interface))
            {
                return true;
            }

            // generic
            var genericTypeDefinition = type.GenericTypeDefinitionOrSelf();
            var genericInterfaceDefinition = @interface.GenericTypeDefinitionOrSelf();

            return TypeInfoStorage.Get(genericTypeDefinition)
                       .DeclaredInterfaces
                       .Select(z => z.GUID)
                       .Contains(genericInterfaceDefinition.GUID)
                   && type.GetGenericArguments().SequenceEqual(@interface.GetGenericArguments());
        }

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="type">Type</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        /// <exception cref="NotFoundException">Throws if source is empty</exception>
        /// <exception cref="AmbiguousMatchException">Throws if source contains more than one element</exception>
        public static TAttribute GetRequiredAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .OfType<TAttribute>()
                .InformativeSingleOrDefault(Amb)
                .EnsureNotNull(() => new AttributeRequiredException(typeof(TAttribute), type));

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Get specified attributes from type
        /// </summary>
        /// <param name="type">Type</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .OfType<TAttribute>();
        }

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="type">Type</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        /// <exception cref="NotFoundException">Throws if source is empty</exception>
        /// <exception cref="AmbiguousMatchException">Throws if source contains more than one element</exception>
        public static TAttribute? GetAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .OfType<TAttribute>()
                .InformativeSingleOrDefault(Amb);

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Does the specified type has an attribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute existence</returns>
        public static bool HasAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .OfType<TAttribute>()
                .Any();
        }

        /// <summary>
        /// Does the specified type has an attribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="attributeType">Attribute type</param>
        /// <returns>Attribute existence</returns>
        public static bool HasAttribute(this Type type, Type attributeType)
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .Any(attributeType.IsInstanceOfType);
        }

        /// <summary> Order types collection by DependencyAttribute </summary>
        /// <param name="source">Unordered types collection</param>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered by DependencyAttribute collection</returns>
        public static IOrderedEnumerable<Type> OrderByDependencyAttribute(this IEnumerable<Type> source)
        {
            return source.OrderByDependencyAttribute(t => t.GenericTypeDefinitionOrSelf());
        }

        /// <summary> Order collection by DependencyAttribute </summary>
        /// <param name="source">Unordered source collection</param>
        /// <param name="accessor">Type accessor</param>
        /// <typeparam name="T">Source items type-argument</typeparam>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered by DependencyAttribute collection</returns>
        public static IOrderedEnumerable<T> OrderByDependencyAttribute<T>(this IEnumerable<T> source, Func<T, Type> accessor)
        {
            int SortFunc(T item)
            {
                var type = accessor(item);
                var dependencies = GetDependenciesByAttribute(type).ToList();

                var depth = 0;

                while (dependencies.Any())
                {
                    if (dependencies.Contains(type))
                    {
                        throw new InvalidOperationException($"{type} has cycle dependency");
                    }

                    ++depth;

                    dependencies = dependencies.SelectMany(GetDependenciesByAttribute).ToList();
                }

                return depth;
            }

            return source.OrderBy(SortFunc);
        }

        /// <summary> Order collection by DependencyAttribute </summary>
        /// <param name="source">Unordered source collection</param>
        /// <param name="getKey">Key accessor</param>
        /// <param name="getDependencies">Dependencies accessor</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TDependency">TDependency type-argument</typeparam>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered by DependencyAttribute collection</returns>
        public static IOrderedEnumerable<TSource> OrderByDependencies<TSource, TDependency>(
            this IEnumerable<TSource> source,
            Func<TSource, TDependency> getKey,
            Func<TSource, IEnumerable<TSource>> getDependencies)
        {
            return source.OrderBy(SortFunc);

            int SortFunc(TSource item)
            {
                var key = getKey(item);
                var dependencies = getDependencies(item).ToList();
                var dependenciesKeys = dependencies.Select(getKey).ToList();

                var depth = 0;

                while (dependenciesKeys.Any())
                {
                    if (dependenciesKeys.Contains(key))
                    {
                        throw new InvalidOperationException($"{key} has cycle dependency");
                    }

                    ++depth;

                    dependencies = dependencies.SelectMany(getDependencies).ToList();
                    dependenciesKeys = dependencies.Select(getKey).ToList();
                }

                return depth;
            }
        }

        /// <summary>
        /// Get type dependencies from DependencyAttribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type dependencies</returns>
        public static IEnumerable<Type> GetDependenciesByAttribute(this Type type)
        {
            return TypeInfoStorage.Get(type).Dependencies;
        }

        /// <summary>
        /// Extract type-arguments from derived class of open-generic type at specified index
        /// Distinct, might be several implementations with different type-arguments
        /// </summary>
        /// <param name="source">Source type for extraction</param>
        /// <param name="openGeneric">Open-generic which derived</param>
        /// <param name="typeArgumentAt">Index of type-argument</param>
        /// <returns>Collection of type arguments at specified index.</returns>
        public static IEnumerable<Type> ExtractGenericArgumentsAt(this Type source, Type openGeneric, int typeArgumentAt = 0)
        {
            if (!openGeneric.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Should be GenericTypeDefinition", nameof(openGeneric));
            }

            if (typeArgumentAt < 0 || typeArgumentAt >= openGeneric.GetGenericArguments().Length)
            {
                throw new ArgumentException("Should be in bounds of generic arguments count", nameof(typeArgumentAt));
            }

            return !IsSubclassOfOpenGeneric(source, openGeneric)
                       ? Enumerable.Empty<Type>()
                       : source
                           .IncludedTypes()
                           .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                           .Select(type => type.GetGenericArguments()[typeArgumentAt])
                           .Distinct();
        }

        /// <summary>
        /// Extract all type-arguments from derived class of open-generic type
        /// </summary>
        /// <param name="source">Source type for extraction</param>
        /// <param name="openGeneric">Open-generic which derived</param>
        /// <returns>Collection of type arguments at specified index.</returns>
        public static IEnumerable<Type[]> ExtractGenericArguments(this Type source, Type openGeneric)
        {
            if (!openGeneric.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Should be GenericTypeDefinition", nameof(openGeneric));
            }

            return !IsSubclassOfOpenGeneric(source, openGeneric)
                       ? Enumerable.Empty<Type[]>()
                       : source
                           .IncludedTypes()
                           .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                           .Select(type => type.GetGenericArguments().ToArray());
        }

        /// <summary>
        /// Unwrap type parameter if possible
        /// </summary>
        /// <param name="typeToUnwrap">Type to unwrap</param>
        /// <param name="openGeneric">Open generic wrapper</param>
        /// <param name="typeArgumentAt">Type argument position</param>
        /// <returns>Unwrapped type</returns>
        public static Type UnwrapTypeParameter(this Type typeToUnwrap, Type openGeneric, int typeArgumentAt = 0)
        {
            return typeToUnwrap.ExtractGenericArgumentsAt(openGeneric, typeArgumentAt).SingleOrDefault()
                   ?? typeToUnwrap;
        }

        /// <summary>
        /// Unwrap type parameters is possible
        /// </summary>
        /// <param name="typeToUnwrap">Type to unwrap</param>
        /// <param name="openGeneric">Open generic wrapper</param>
        /// <returns>Unwrapped type</returns>
        public static Type[] UnwrapTypeParameters(this Type typeToUnwrap, Type openGeneric)
        {
            return typeToUnwrap.ExtractGenericArguments(openGeneric).Single();
        }

        /// <summary>
        /// Base types from source type
        /// - base types
        /// - interfaces
        /// </summary>
        /// <param name="source">Source type</param>
        /// <returns>Included types</returns>
        public static IEnumerable<Type> BaseTypes(this Type source)
        {
            return TypeInfoStorage.Get(source).BaseTypes
                .Concat(source.GetInterfaces());
        }

        /// <summary>
        /// Types included in source type
        /// - source
        /// - base types
        /// - interfaces
        /// </summary>
        /// <param name="source">Source type</param>
        /// <returns>Included types</returns>
        public static IEnumerable<Type> IncludedTypes(this Type source)
        {
            return new[] { source }.Concat(source.BaseTypes());
        }

        /// <summary>
        /// Extract GenericTypeDefinition or return argument type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>GenericTypeDefinition or argument type</returns>
        public static Type GenericTypeDefinitionOrSelf(this Type type)
        {
            return type.IsGenericType
                       ? type.GetGenericTypeDefinition()
                       : type;
        }

        /// <summary>
        /// Does type fits for type argument
        /// </summary>
        /// <param name="typeForCheck">Actual type for check</param>
        /// <param name="typeArgument">Type argument</param>
        /// <returns>True - fits / False doesn't fits</returns>
        public static bool FitsForTypeArgument(this Type typeForCheck, Type typeArgument)
        {
            if (!typeArgument.IsGenericParameter)
            {
                if (!typeArgument.IsGenericType)
                {
                    return typeArgument.IsAssignableFrom(typeForCheck);
                }

                var genericParameters = typeArgument.GetGenericArguments();

                return typeForCheck
                    .ExtractGenericArguments(typeArgument.GetGenericTypeDefinition())
                    .Any(set => set
                            .Zip(genericParameters, (arg, param) => (arg, param))
                            .All(pair => pair.arg.FitsForTypeArgument(pair.param)));
            }

            var byConstraints = typeArgument
                .GetGenericParameterConstraints()
                .All(CheckConstraint);

            var filters = GetFiltersByTypeParameterAttributes(typeArgument.GenericParameterAttributes);
            var byGenericParameterAttributes = filters.All(filter => filter(typeForCheck));

            return byConstraints && byGenericParameterAttributes;

            bool CheckConstraint(Type constraint)
            {
                if (!constraint.IsGenericType)
                {
                    return constraint.IsAssignableFrom(typeForCheck);
                }

                var constraintGenericTypeDefinition = constraint.GetGenericTypeDefinition();

                return IsSubclassOfOpenGeneric(typeForCheck, constraintGenericTypeDefinition)
                       && HasSuitableTypeArguments();

                bool HasSuitableTypeArguments()
                {
                    var typeArgumentsForCheck = ExtractGenericArguments(typeForCheck, constraintGenericTypeDefinition).ToArray();

                    return constraint
                        .GetGenericArguments()
                        .Select((constraintGenericArgument, position) => (constraintGenericArgument, position))
                        .All(info => typeArgumentsForCheck
                            .Any(typeArgumentForCheck => info.constraintGenericArgument == typeArgument
                                                         ? typeForCheck == typeArgumentForCheck[info.position] // T : IService<T>
                                                         : FitsForTypeArgument(typeArgumentForCheck[info.position], info.constraintGenericArgument)));
                }
            }

            static ICollection<Func<Type, bool>> GetFiltersByTypeParameterAttributes(GenericParameterAttributes genericParameterAttributes)
            {
                var filters = new List<Func<Type, bool>>();

                if (genericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    filters.Add(type => type.IsClass || type.IsInterface);
                }

                if (genericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    filters.Add(type => type.IsValueType && !type.IsNullable());
                }

                if (genericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    filters.Add(type =>
                    {
                        if (type.IsValueType)
                        {
                            return true;
                        }

                        var ctor = type.GetConstructor(Array.Empty<Type>());

                        return ctor != null;
                    });
                }

                if (!filters.Any())
                {
                    filters.Add(type => true);
                }

                return filters;
            }
        }

        private static IEnumerable<Type> SelfDependencies(this ConstructorInfo cctor, Type dependency)
        {
            return cctor
                .DeclaringType
                .BaseTypes()
                .Where(type => type.GenericTypeDefinitionOrSelf() == dependency.GenericTypeDefinitionOrSelf());
        }
    }
}