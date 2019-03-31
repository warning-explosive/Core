namespace SpaceEngineers.Core.Utilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using EqualityComparers;

    /// <summary>
    /// Object DeepCopy extension methods
    /// </summary>
    public static partial class ObjectExtensions
    {
        private static readonly MethodInfo ShallowCopyMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Get shallow copy of object
        /// Dont copy internal reference links
        /// </summary>
        /// <param name="original">Original object</param>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Shallow copy of original object</returns>
        public static T ShallowCopy<T>(this T original)
        {
            return (T)ShallowCopy(original as object);
        }
        
        /// <summary>
        /// Get deep copy of object (by reflection)
        /// Copy all internal reference links except System.String, System.Type, System.ValueType
        /// </summary>
        /// <param name="original">Original object</param>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Deep copy of original object</returns>
        public static T DeepCopy<T>(this T original)
        {
            return (T)DeepCopy(original as object);
        }
        
        /// <summary>
        /// Get deep copy of object (by serialization)
        /// Copy all internal reference links
        /// </summary>
        /// <param name="original">Original object</param>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Deep copy of original object</returns>
        /// <remarks>https://docs.microsoft.com/en-us/dotnet/standard/serialization/binary-serialization</remarks>
        public static T DeepCopyBySerialization<T>(this T original) where T : new()
        {
            return (T)DeepCopyBySerialization(original as object);
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

        /// <summary>
        /// Get deep copy of object (by reflection)
        /// Copy all internal reference links except System.String, System.Type, System.ValueType
        /// </summary>
        /// <param name="original">Original object</param>
        /// <returns>Deep copy of original object</returns>
        public static object DeepCopy(this object original)
        {
            return original.DeepCopyInternal(new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }

        /// <summary>
        /// Get deep copy of object (by serialization)
        /// Copy all internal reference links
        /// </summary>
        /// <param name="original">Original object</param>
        /// <returns>Deep copy of original object</returns>
        /// <remarks>https://docs.microsoft.com/en-us/dotnet/standard/serialization/binary-serialization</remarks>
        public static object DeepCopyBySerialization(this object original)
        {
            using (var stream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, original);
                stream.Seek(0, SeekOrigin.Begin);
                return binaryFormatter.Deserialize(stream);
            }
        }

        private static object DeepCopyInternal(this object original, IDictionary<object, object> visited)
        {
            if (original == null)
            {
                return null;
            }
            
            var typeToReflect = original.GetType();

            if (typeToReflect.IsPrimitive())
            {
                return original;
            }

            if (typeToReflect == typeof(string))
            {
                return string.Copy((string)original);
            }

            if (visited.ContainsKey(original))
            {
                return visited[original];
            }

            var clone = original.ShallowCopy();
            
            // add here to prevent cyclic reference on yourself
            visited.Add(original, clone);
            
            clone.ProcessClone(original, typeToReflect, visited);
            
            return clone;
        }

        private static bool IsPrimitive(this Type type)
        {
            return (type.IsPrimitive | type.IsEnum)
                   || type == typeof(Type);
        }

        private static object ProcessClone(this object clone,
                                           object original,
                                           Type typeToReflect,
                                           IDictionary<object, object> visited)
        {
            if (clone == null)
            {
                return null;
            }

            if (typeToReflect.IsArray)
            {
                if (!typeToReflect.GetElementType().IsPrimitive())
                {
                    var arrayClone = (Array)clone;

                    for (var index = 0; index < arrayClone.Length; ++index)
                    {
                        arrayClone.SetValue(arrayClone.GetValue(index).DeepCopyInternal(visited), index);
                    }
                }
            }
            else
            {
                original
                    .CopyFields(typeToReflect, clone, visited)
                    .CopyBaseTypeFields(typeToReflect, clone, visited);
            }

            return clone;
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
                if (fieldInfo.FieldType.IsPrimitive())
                {
                    continue;
                }
                
                fieldInfo.SetValue(clone, fieldInfo.GetValue(original).DeepCopyInternal(visited));
            }

            return original;
        }

        private static object CopyBaseTypeFields(this object original,
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

            return original;
        }
    }
}