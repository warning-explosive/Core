namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Reflection;
    using Abstractions;

    internal interface ITypeInfoStorage : IResolvable
    {
        Extensions.TypeInfo this[Type type] { get; }

        bool ContainsKey(Type type);

        Assembly[] OurAssemblies { get; }

        Type[] OurTypes { get; }

        Type[] AllLoadedTypes { get; }
    }
}