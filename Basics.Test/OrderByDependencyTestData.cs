namespace SpaceEngineers.Core.Basics.Test
{
    internal class OrderByDependencyTestData
    {
        /*
         * non generic
         */
        [Attributes.Dependency(typeof(DependencyTest2))]
        internal class DependencyTest1 { }

        [Attributes.Dependency(typeof(DependencyTest3))]
        internal class DependencyTest2 { }

        internal class DependencyTest3 { }

        /*
         * generic
         */
        [Attributes.Dependency(typeof(GenericDependencyTest2<>))]
        internal class GenericDependencyTest1<T> { }

        [Attributes.Dependency(typeof(GenericDependencyTest3<>))]
        internal class GenericDependencyTest2<T> { }

        internal class GenericDependencyTest3<T> { }

        /*
         * cycle dependency
         */
        [Attributes.Dependency(typeof(CycleDependencyTest2))]
        internal class CycleDependencyTest1 { }

        [Attributes.Dependency(typeof(CycleDependencyTest3))]
        internal class CycleDependencyTest2 { }

        [Attributes.Dependency(typeof(CycleDependencyTest1))]
        internal class CycleDependencyTest3 { }
    }
}