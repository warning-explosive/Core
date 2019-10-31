namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System;
    using System.Reflection;
    using TypeInfo = TypeInfo;

    internal interface ITypeInfoStorage : IResolvable
    {
        TypeInfo this[Type type] { get; }

        bool ContainsKey(Type type);

        Assembly[] OurAssemblies { get; }

        Type[] OurTypes { get; }

        Type[] AllLoadedTypes { get; }
    }
}