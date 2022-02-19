namespace SpaceEngineers.Core.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;

    /// <summary>
    /// DynamicClass
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public sealed class DynamicClass : IEquatable<DynamicClass>,
                                       ISafelyEquatable<DynamicClass>,
                                       ICloneable<DynamicClass>
    {
        private const string DynamicAssemblyName = "SpaceEngineers.Core.Basics.Dynamic";

        /// <summary> .cctor </summary>
        public DynamicClass()
            : this(DynamicAssemblyName, GenerateClassName())
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="className">Class name</param>
        public DynamicClass(string className)
            : this(DynamicAssemblyName, className)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Class name</param>
        public DynamicClass(string assemblyName, string className)
        {
            AssemblyName = assemblyName;
            ClassName = className;
            Interfaces = new List<Type>();
            Properties = new List<DynamicProperty>();
        }

        /// <summary>
        /// Assembly name
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Base type
        /// </summary>
        public Type? BaseType { get; private set; }

        /// <summary>
        /// Interfaces
        /// </summary>
        public IReadOnlyCollection<Type> Interfaces { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        public IReadOnlyCollection<DynamicProperty> Properties { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left DynamicClass</param>
        /// <param name="right">Right DynamicClass</param>
        /// <returns>equals</returns>
        public static bool operator ==(DynamicClass? left, DynamicClass? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left DynamicClass</param>
        /// <param name="right">Right DynamicClass</param>
        /// <returns>not equals</returns>
        public static bool operator !=(DynamicClass? left, DynamicClass? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return new object[] { BaseType ?? typeof(object) }
                .Concat(Interfaces)
                .Concat(Properties)
                .Aggregate(
                    HashCode.Combine(
                        AssemblyName.GetHashCode(StringComparison.OrdinalIgnoreCase),
                        ClassName.GetHashCode(StringComparison.OrdinalIgnoreCase)),
                    HashCode.Combine);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(DynamicClass? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(DynamicClass other)
        {
            return AssemblyName.Equals(other.AssemblyName, StringComparison.OrdinalIgnoreCase)
                   && ClassName.Equals(other.ClassName, StringComparison.OrdinalIgnoreCase)
                   && BaseType == other.BaseType
                   && Interfaces.OrderBy(i => i).SequenceEqual(other.Interfaces.OrderBy(i => i))
                   && Properties.OrderBy(p => p.Name).SequenceEqual(other.Properties.OrderBy(p => p.Name));
        }

        #endregion

        #region Fluent API

        /// <summary>
        /// Inherits from specified base type
        /// </summary>
        /// <param name="baseType">Base type</param>
        /// <returns>DynamicClass with specified base type</returns>
        public DynamicClass InheritsFrom(Type baseType)
        {
            var copy = Clone();
            copy.BaseType = baseType;
            return copy;
        }

        /// <summary>
        /// Implements specified interfaces
        /// </summary>
        /// <param name="interfaces">Interfaces</param>
        /// <returns>DynamicClass with specified interfaces</returns>
        public DynamicClass Implements(params Type[] interfaces)
        {
            if (!interfaces.Any())
            {
                return this;
            }

            var copy = Clone();
            copy.Interfaces = copy.Interfaces.Concat(interfaces).ToList();
            return copy;
        }

        /// <summary>
        /// Defines dynamic properties
        /// </summary>
        /// <param name="dynamicProperties">Dynamic properties</param>
        /// <returns>DynamicClass with specified dynamic properties</returns>
        public DynamicClass HasProperties(params DynamicProperty[] dynamicProperties)
        {
            if (!dynamicProperties.Any())
            {
                return this;
            }

            var copy = Clone();
            copy.Properties = copy.Properties.Concat(dynamicProperties).ToList();
            return copy;
        }

        #endregion

        /// <inheritdoc />
        public DynamicClass Clone()
        {
            return new DynamicClass(AssemblyName, ClassName)
            {
                BaseType = BaseType,
                Interfaces = Interfaces.ToList(),
                Properties = Properties.ToList()
            };
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return Clone();
        }

        private static string GenerateClassName()
        {
            return string.Join(
                "_",
                nameof(DynamicClass),
                Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase));
        }
    }
}