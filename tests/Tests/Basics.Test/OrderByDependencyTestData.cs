namespace SpaceEngineers.Core.Basics.Test
{
    using Attributes;

    internal class OrderByDependencyTestData
    {
        /*
         * non generic
         */
        [After(typeof(DependencyTest2))]
        internal class DependencyTest1 { }

        [After(typeof(DependencyTest3))]
        internal class DependencyTest2 { }

        internal class DependencyTest3 { }

        [Before(typeof(DependencyTest3))]
        internal class DependencyTest4 { }

        /*
         * weakly typed
         */
        [After("SpaceEngineers.Core.Basics.Test SpaceEngineers.Core.Basics.Test.OrderByDependencyTestData+WeakDependencyTest2")]
        internal class WeakDependencyTest1 { }

        [After("SpaceEngineers.Core.Basics.Test SpaceEngineers.Core.Basics.Test.OrderByDependencyTestData+WeakDependencyTest3")]
        internal class WeakDependencyTest2 { }

        internal class WeakDependencyTest3 { }

        [Before("SpaceEngineers.Core.Basics.Test SpaceEngineers.Core.Basics.Test.OrderByDependencyTestData+WeakDependencyTest3")]
        internal class WeakDependencyTest4 { }

        /*
         * generic
         */
        [After(typeof(GenericDependencyTest2<>))]
        internal class GenericDependencyTest1<T> { }

        [After(typeof(GenericDependencyTest3<>))]
        internal class GenericDependencyTest2<T> { }

        internal class GenericDependencyTest3<T> { }

        [Before(typeof(GenericDependencyTest3<>))]
        internal class GenericDependencyTest4<T> { }

        /*
         * cycle dependency
         */
        [After(typeof(CycleDependencyTest2))]
        internal class CycleDependencyTest1 { }

        [After(typeof(CycleDependencyTest3))]
        internal class CycleDependencyTest2 { }

        internal class CycleDependencyTest3 { }

        [Before(typeof(CycleDependencyTest3))]
        [After(typeof(CycleDependencyTest1))]
        internal class CycleDependencyTest4 { }
    }
}