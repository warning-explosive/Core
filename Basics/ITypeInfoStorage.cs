namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Reflection;

    internal interface ITypeInfoStorage
    {
        TypeInfo this[Type type] { get; }

        bool ContainsKey(Type type);

        Assembly[] OurAssemblies { get; }

        Type[] OurTypes { get; }

        Type[] AllLoadedTypes { get; }
    }
}