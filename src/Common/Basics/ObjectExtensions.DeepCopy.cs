namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using EqualityComparers;

    /// <summary>
    /// Object DeepCopy extension methods
    /// </summary>
    public static partial class ObjectExtensions
    {
        // ReSharper disable once PossibleMistakenCallToGetType.2
        private static readonly Type RuntimeType = typeof(Type).GetType();

        private static readonly MethodInfo ShallowCopyMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Get shallow copy of object
        /// Dont copy internal reference links
        /// </summary>
        /// <param name="original">Original object</param>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Shallow copy of original object</returns>
        public static T ShallowCopy<T>(this T original)
            where T : class
        {
            return (T)ShallowCopy((object)original);
        }

        /// <summary>
        /// Get deep copy of object (by reflection)
        /// Copy all internal reference links except System.String, System.Type, System.ValueType
        /// </summary>
        /// <param name="original">Original object</param>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Deep copy of original object</returns>
        public static T DeepCopy<T>([DisallowNull] this T original)
        {
            return (T?)original.DeepCopyInternal(new Dictionary<object, object>(new ReferenceEqualityComparer<object>()))
                   ?? throw new InvalidOperationException("Not nullable input should be copied into not nullable output");
        }

        /// <summary>
        /// Get shallow copy of object
        /// Dont copy internal reference links
        /// </summary>
        /// <param name="original">Original object</param>
        /// <returns>Shallow copy of original object</returns>
        public static object ShallowCopy(this object original)
        {
            return ShallowCopyMethod.Invoke(original, null);
        }

        private static object? DeepCopyInternal(this object? original, IDictionary<object, object> visited)
        {
            if (original == null)
            {
                return null;
            }

            var typeToReflect = original.GetType();

            if (visited.ContainsKey(original))
            {
                return visited[original];
            }

            if (IsPrimitive(typeToReflect))
            {
                return original;
            }

            var isCloneable = typeToReflect.IsSubclassOfOpenGeneric(typeof(ICloneable<>));

            object clone;

            if (isCloneable)
            {
                clone = original
                    .CallMethod(nameof(ICloneable<object>.Clone))
                    .Invoke<object>();

                visited.Add(original, clone);
            }
            else
            {
                clone = original.ShallowCopy();

                /*
                 * prevent cyclic reference on yourself
                 */
                visited.Add(original, clone);

                clone.ProcessClone(original, typeToReflect, visited);
            }

            return clone;
        }

        private static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive() || type == RuntimeType;
        }

        private static void ProcessClone(this object? clone, object original, Type typeToReflect, IDictionary<object, object> visited)
        {
            if (clone == null)
            {
                return;
            }

            if (typeToReflect.IsArray())
            {
                var arrayClone = (Array)clone;

                for (var index = 0; index < arrayClone.Length; ++index)
                {
                    arrayClone.SetValue(arrayClone.GetValue(index).DeepCopyInternal(visited), index);
                }
            }
            else
            {
                original.CopyFields(typeToReflect, clone, visited)
                        .CopyBaseTypeFields(typeToReflect, clone, visited);
            }
        }

        private static object CopyFields(this object original,
                                         Type typeToReflect,
                                         object clone,
                                         IDictionary<object, object> visited)
        {
            foreach (var fieldInfo in typeToReflect.GetFields(BindingFlags.Instance
                                                              | BindingFlags.Public
                                                              | BindingFlags.NonPublic
                                                              | BindingFlags.DeclaredOnly))
            {
                fieldInfo.SetValue(clone,
                                   IsPrimitive(fieldInfo.FieldType)
                                       ? fieldInfo.GetValue(original)
                                       : fieldInfo.GetValue(original).DeepCopyInternal(visited));
            }

            return original;
        }

        private static void CopyBaseTypeFields(this object original,
                                               Type typeToReflect,
                                               object clone,
                                               IDictionary<object, object> visited)
        {
            if (typeToReflect.BaseType != null)
            {
                original
                   .CopyFields(typeToReflect.BaseType, clone, visited)
                   .CopyBaseTypeFields(typeToReflect.BaseType, clone, visited);
            }
        }
    }
}