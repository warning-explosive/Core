namespace SpaceEngineers.Core.Dynamic.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class DynamicClassProvider : IDynamicClassProvider,
                                        IResolvable<IDynamicClassProvider>
    {
        private const TypeAttributes DynamicClassTypeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;
        private const MethodAttributes GetSetMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        private static readonly ConcurrentDictionary<string, ModuleBuilder> ModulesCache = new ConcurrentDictionary<string, ModuleBuilder>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<DynamicClass, Type> TypesCache = new ConcurrentDictionary<DynamicClass, Type>();

        public object CreateInstance(DynamicClass dynamicClass, IReadOnlyDictionary<DynamicProperty, object?> values)
        {
            var type = CreateType(dynamicClass);

            var instance = Activator.CreateInstance(type);

            return dynamicClass
                .Properties
                .Aggregate(instance, ApplyValue);

            object ApplyValue(object acc, DynamicProperty dynamicProperty)
            {
                if (values.TryGetValue(dynamicProperty, out var value))
                {
                    type.GetProperty(dynamicProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                       ?.SetValue(acc, value);
                }

                return acc;
            }
        }

        public Type CreateType(DynamicClass dynamicClass)
        {
            return TypesCache.GetOrAdd(dynamicClass, BuildClass);
        }

        private static Type BuildClass(DynamicClass dynamicClass)
        {
            var moduleBuilder = ModulesCache.GetOrAdd(dynamicClass.AssemblyName, BuildModule);

            var typeBuilder = moduleBuilder.DefineType(
                dynamicClass.ClassName,
                DynamicClassTypeAttributes,
                dynamicClass.BaseType,
                dynamicClass.Interfaces.ToArray());

            foreach (var dynamicProperty in dynamicClass.Properties)
            {
                DefineAutoProperty(typeBuilder, dynamicProperty);
            }

            return typeBuilder.CreateType();
        }

        private static ModuleBuilder BuildModule(string assemblyName)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName);
        }

        private static void DefineAutoProperty(TypeBuilder typeBuilder, DynamicProperty dynamicProperty)
        {
            var propertyBuilder = typeBuilder.DefineProperty(
                dynamicProperty.Name,
                PropertyAttributes.HasDefault,
                dynamicProperty.Type,
                null);

            var backingField = typeBuilder.DefineField(
                GetBackingFieldName(dynamicProperty),
                dynamicProperty.Type,
                FieldAttributes.Private);

            propertyBuilder.SetGetMethod(DefineGetMethod(dynamicProperty, typeBuilder, backingField));
            propertyBuilder.SetSetMethod(DefineSetMethod(dynamicProperty, typeBuilder, backingField));
        }

        private static string GetBackingFieldName(DynamicProperty dynamicProperty)
        {
            return string.Join(
                string.Empty,
                "_",
                char.ToLowerInvariant(dynamicProperty.Name[0]),
                dynamicProperty.Name.Substring(1));
        }

        private static MethodBuilder DefineGetMethod(
            DynamicProperty dynamicProperty,
            TypeBuilder typeBuilder,
            FieldBuilder backingField)
        {
            var getMethodBuilder = typeBuilder.DefineMethod(
                $"get_{dynamicProperty.Name}",
                GetSetMethodAttributes,
                dynamicProperty.Type,
                Array.Empty<Type>());

            var getMethodILGenerator = getMethodBuilder.GetILGenerator();

            getMethodILGenerator.Emit(OpCodes.Ldarg_0);
            getMethodILGenerator.Emit(OpCodes.Ldfld, backingField);
            getMethodILGenerator.Emit(OpCodes.Ret);
            return getMethodBuilder;
        }

        private static MethodBuilder DefineSetMethod(
            DynamicProperty dynamicProperty,
            TypeBuilder typeBuilder,
            FieldBuilder backingField)
        {
            var setMethodBuilder = typeBuilder.DefineMethod(
                $"set_{dynamicProperty.Name}",
                GetSetMethodAttributes,
                null,
                new[] { dynamicProperty.Type });

            ILGenerator setMethodILGenerator = setMethodBuilder.GetILGenerator();

            setMethodILGenerator.Emit(OpCodes.Ldarg_0);
            setMethodILGenerator.Emit(OpCodes.Ldarg_1);
            setMethodILGenerator.Emit(OpCodes.Stfld, backingField);
            setMethodILGenerator.Emit(OpCodes.Ret);

            return setMethodBuilder;
        }
    }
}