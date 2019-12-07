namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Reflection;

    internal interface ITypeInfoStorage
    {
        Assembly[] OurAssemblies { get; }

        Type[] OurTypes { get; }

        Type[] AllLoadedTypes { get; }

        TypeInfo this[Type type] { get; }

        bool ContainsKey(Type type);
    }
}