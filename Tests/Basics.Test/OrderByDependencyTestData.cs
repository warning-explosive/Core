namespace SpaceEngineers.Core.Basics.Test
{
    using Attributes;

    internal class OrderByDependencyTestData
    {
        /*
         * non generic
         */
        [Dependency(typeof(DependencyTest2))]
        internal class DependencyTest1 { }

        [Dependency(typeof(DependencyTest3))]
        internal class DependencyTest2 { }

        internal class DependencyTest3 { }

        [Dependent(typeof(DependencyTest3))]
        internal class DependencyTest4 { }

        /*
         * weakly typed
         */
        [Dependency("SpaceEngineers.Core.Basics.Test.OrderByDependencyTestData.WeakDependencyTest2")]
        internal class WeakDependencyTest1 { }

        [Dependency("SpaceEngineers.Core.Basics.Test.OrderByDependencyTestData.WeakDependencyTest3")]
        internal class WeakDependencyTest2 { }

        internal class WeakDependencyTest3 { }

        [Dependent("SpaceEngineers.Core.Basics.Test.OrderByDependencyTestData.WeakDependencyTest3")]
        internal class WeakDependencyTest4 { }

        /*
         * generic
         */
        [Dependency(typeof(GenericDependencyTest2<>))]
        internal class GenericDependencyTest1<T> { }

        [Dependency(typeof(GenericDependencyTest3<>))]
        internal class GenericDependencyTest2<T> { }

        internal class GenericDependencyTest3<T> { }

        [Dependent(typeof(GenericDependencyTest3<>))]
        internal class GenericDependencyTest4<T> { }

        /*
         * cycle dependency
         */
        [Dependency(typeof(CycleDependencyTest2))]
        internal class CycleDependencyTest1 { }

        [Dependency(typeof(CycleDependencyTest3))]
        internal class CycleDependencyTest2 { }

        internal class CycleDependencyTest3 { }

        [Dependent(typeof(CycleDependencyTest3))]
        [Dependency(typeof(CycleDependencyTest1))]
        internal class CycleDependencyTest4 { }
    }
}